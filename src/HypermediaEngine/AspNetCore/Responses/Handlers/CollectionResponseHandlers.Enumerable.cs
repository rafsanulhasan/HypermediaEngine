using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SynergyFx.EntityTagCaching.Models;
using SynergyFx.HypermediaEngine.Abstractions;
using SynergyFx.HypermediaEngine.Responses.Metadata;
using SynergyFx.HypermediaEngine.Responses.Rules;

namespace SynergyFx.HypermediaEngine.Responses.Handlers;

#pragma warning disable MA0048 // File name must match type name
internal sealed class EnumerableCollectionResponseHandler<T>(
    IHttpContextAccessor httpContextAccessor,
    [FromKeyedServices(CollectonPipelineRules.CollectionRuleName)] ICollectionResponsePipeline<T> enumerablePipeline,
    IEnumerable<AbstractCollectionMetadataHandler<T>> metadataHandlers,
    IEnumerable<AbstractCollectionLinkHandler<T>> linkHandlers,
    IHypermediaCollectionBuilder<T> builder,
    ILogger<EnumerableCollectionResponseHandler<T>> logger
) : AbstractCollectionResponseHandler<T, IEnumerable<T>>(httpContextAccessor, metadataHandlers, linkHandlers, builder)
    where T : notnull
{
    public override async ValueTask<object?> HandleCollectionResponseAsync(IEnumerable<T> response)
    {
        await WithPopulatedQueryParamsAsync().ConfigureAwait(false);

        CollectionResponseContext<T> context = new(response)
        {
            QueryParams = QueryParams,
        };

        context = await enumerablePipeline.Process(context).ConfigureAwait(false);
        Builder = context.FinalItems.Match(
            finalItems => Builder.WithItems(finalItems),
            () => Builder.WithItems(context.Items)
        );
        ListResponseMetadata metadata = new(EntityTag.Empty)
        {
            Filters = context.Filter.Flatten(),
            Paging = context.PagingMetadata.Flatten(),
            Sorting = context.SortingMetadata.Flatten<SortingMetadata, IReadOnlyList<SortingMetadata>>(),
        };
        Builder = Builder.WithMetadata(metadata);
        ApplyMetadata(response, metadata);
        ApplyLinks();
        return Builder.Build();
    }
}
#pragma warning restore MA0048 // File name must match type name
