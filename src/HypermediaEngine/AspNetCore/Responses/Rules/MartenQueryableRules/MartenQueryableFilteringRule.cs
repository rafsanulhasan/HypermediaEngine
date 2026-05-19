using HypermediaEngine.Abstractions;
using HypermediaEngine.Helpers;
using HypermediaEngine.Requests.Filtering;
using HypermediaEngine.Responses.Metadata;

using LanguageExt;

using Marten;
using Marten.Linq;

namespace HypermediaEngine.Responses.Rules.MartenQueryableRules;

internal sealed class MartenQueryableFilteringRule<T>(ICollectionResponsePipeline<T> next)
    : ICollectionResponsePipeline<T>
    where T : notnull
{
    public async ValueTask<CollectionResponseContext<T>> Process(CollectionResponseContext<T> context)
    {
        IQueryable<T> filteredQuery = context.Query.Match(
            Some: q =>
            {
                IMartenQueryable<T> marten = q as IMartenQueryable<T>
                    ?? throw new InvalidOperationException(
                        $"Query must be an {nameof(IMartenQueryable<T>)} instance for the Marten pipeline.");
                FilterNode? filterNode = context.Filter.Match<FilterNode?>(
                    Some: filter => filter,
                    None: () => null);
                return marten.ApplyFiltering(filterNode);
            },
            None: () => throw new InvalidOperationException(
                "CollectionResponseContext.Query must be set before the MartenQueryable filtering rule."));

        int totalCount = await filteredQuery.CountAsync().ConfigureAwait(false);

        PagingMetadata pagingMetadata = context.PagingMetadata.Match(
            Some: paging => paging with
            {
                TotalCount = totalCount,
            },
            None: () => new PagingMetadata()
            {
                TotalCount = totalCount,
            });

        context = context with
        {
            FilteredQuery = Option<IQueryable<T>>.Some(filteredQuery),
            PagingMetadata = pagingMetadata,
        };

        return await next.Process(context).ConfigureAwait(false);
    }
}
