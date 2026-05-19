using HypermediaEngine.Abstractions;
using HypermediaEngine.Helpers;
using HypermediaEngine.Requests;
using HypermediaEngine.Requests.Paging;
using HypermediaEngine.Requests.Sorting;
using HypermediaEngine.Responses.Metadata;

using LanguageExt;

namespace HypermediaEngine.Responses.Rules.MartenQueryableRules;

internal sealed class MartenQueryableSortingRule<T>(ICollectionResponsePipeline<T> next)
    : ICollectionResponsePipeline<T>
    where T : notnull
{
    public async ValueTask<CollectionResponseContext<T>> Process(CollectionResponseContext<T> context)
    {
        IQueryable<T> pagedQuery = context.PagedQuery.Match(
            Some: q => q,
            None: () => throw new InvalidOperationException(
                "CollectionResponseContext.PagedQuery must be set before the MartenQueryable sorting rule."));

        (IQueryable<T> sortedQuery, QueryParams queryParams) = context.QueryParams.Match(
            Some: queryParams =>
            {
                IReadOnlyList<SortField>? sorts = queryParams.Body?.Sorting;
                return (pagedQuery.ApplySorting(sorts), queryParams);
            },
            None: () => (pagedQuery, new QueryParams() { Paging = OffsetOrCursorPaging.Default })
        );

        List<SortingMetadata> sortingMetadata = queryParams.Body is not { Sorting.Count: > 0 }
            ? []
            : [.. queryParams.Body.Sorting.Select(s => new SortingMetadata(s.Field, s.Direction))];

        context = context with
        {
            SortedQuery = Option<IQueryable<T>>.Some(sortedQuery),
            SortingMetadata = Option<IReadOnlyList<SortingMetadata>>.Some(sortingMetadata),
        };

        return await next.Process(context).ConfigureAwait(false);
    }
}
