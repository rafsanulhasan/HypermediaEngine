using HypermediaEngine.Requests;

using LanguageExt;

using SynergyFx.HypermediaEngine.Abstractions;
using SynergyFx.HypermediaEngine.Requests.Paging;
using SynergyFx.HypermediaEngine.Requests.Sorting;
using SynergyFx.HypermediaEngine.Responses.Metadata;

namespace SynergyFx.HypermediaEngine.Responses.Rules.QueryableRules;

internal sealed class QueryableSortingRule<T>(ICollectionResponsePipeline<T> next)
    : ICollectionResponsePipeline<T>
    where T : notnull
{
    public async ValueTask<CollectionResponseContext<T>> Process(CollectionResponseContext<T> context)
    {
        IQueryable<T> pagedQuery = context.PagedQuery.Match(
            Some: q => q,
            None: () => Enumerable.Empty<T>().AsQueryable());

        (IQueryable<T> sortedQuery, QueryParams queryParams) = context.QueryParams.Match(
            Some: queryParams =>
            {
                if (queryParams.Body is not { Sorting.Count: > 0 })
                {
                    return (pagedQuery, queryParams);
                }

                IQueryable<T> sorted = pagedQuery;
                foreach (SortField sort in queryParams.Body.Sorting)
                {
                    sorted = sorted.OrderByDynamic(
                        "{Field} {Direction}",
                        new { sort.Field, sort.Direction });
                }
                return (sorted, queryParams);
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
