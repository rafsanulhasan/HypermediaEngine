using EntityTagCaching.Models;

using HypermediaEngine.Abstractions;
using HypermediaEngine.Requests;
using HypermediaEngine.Requests.Filtering;
using HypermediaEngine.Requests.Paging;
using HypermediaEngine.Responses.Metadata;
using HypermediaEngine.Responses.Rules;

using Marten.Linq;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HypermediaEngine.Responses.Handlers;

internal sealed class MartenQueryableResponseHandler<T>(
    IHttpContextAccessor httpContextAccessor,
    [FromKeyedServices(CollectonPipelineRules.MartenQueryableRuleName)] ICollectionResponsePipeline<T> queryablePipeline,
    IEnumerable<AbstractCollectionMetadataHandler<T>> metadataHandlers,
    IEnumerable<AbstractCollectionLinkHandler<T>> linkHandlers,
    IHypermediaCollectionBuilder<T> builder,
    ICollectionResponseHandler<T> nextHandler,
    ILogger<MartenQueryableResponseHandler<T>> logger
) : AbstractCollectionResponseHandler<T, IMartenQueryable<T>>(httpContextAccessor, metadataHandlers, linkHandlers, builder, nextHandler)
    where T : notnull
{
    public override async ValueTask<object?> HandleCollectionResponseAsync(IMartenQueryable<T> response)
    {
        using var handler = await WithPopulatedQueryParamsAsync().ConfigureAwait(false);

        QueryParams ??= handler.QueryParams
                     ?? new QueryParams(paging: OffsetPaging.Default);

        CollectionResponseContext<T> context = new(response)
        {
            QueryParams = QueryParams,
        };

        context = await queryablePipeline.Process(context).ConfigureAwait(false);

        Builder = context.FinalItems.Match(
            finalItems => Builder.WithItems(finalItems),
            () => Builder.WithItems(context.Items));
        ListResponseMetadata metadata = new(EntityTag.Empty)
        {
            Filters = context.Filter.Match<FilterNode?>(node => node, () => null),
            Paging = context.PagingMetadata.Match<PagingMetadata?>(meta => meta, () => null),
            Sorting = context.SortingMetadata.Match<IReadOnlyList<SortingMetadata>?>(node => node, () => null),
        };
        Builder = Builder.WithMetadata(metadata);
        ApplyMetadata(response, metadata);
        ApplyLinks();
        return Builder.Build();
    }
}
