using HypermediaEngine.Abstractions;
using HypermediaEngine.Requests;
using HypermediaEngine.Requests.Paging;
using HypermediaEngine.Responses.Metadata;

using LanguageExt;

namespace HypermediaEngine.Responses.Rules.EnumerableRules;

internal sealed class EnumerableSortingRule<T>(ICollectionResponsePipeline<T> next)
    : ICollectionResponsePipeline<T>
    where T : notnull
{
    public async ValueTask<CollectionResponseContext<T>> Process(CollectionResponseContext<T> context)
    {
        IEnumerable<T> pagedItems = context.PagedItems.Match(
            Some: items => items,
            None: () => context.PagedItems
                .Match(Some: items => items, None: () => context.Items)
        );

        (IEnumerable<T> sortedItems, QueryParams queryParams) = context.QueryParams.Match(
            Some: queryParams =>
            {
                if (queryParams.Body is not { Sorting.Count: > 0 })
                {
                    return (pagedItems, queryParams);
                }

                IEnumerable<T> sorted = pagedItems;
                foreach (var sort in queryParams.Body.Sorting)
                {
                    sorted = sorted.OrderByDynamic(
                        "{Field} {Direction}",
                        new { sort.Field, sort.Direction });
                }
                return (sorted, queryParams);
            },
            None: () => (pagedItems, new QueryParams() { Paging = OffsetOrCursorPaging.Default })
        );

        List<SortingMetadata> sortingMetadata = queryParams.Body is not { Sorting.Count: > 0 }
            ? []
            : [.. queryParams.Body.Sorting.Select(s => new SortingMetadata(s.Field, s.Direction))];

        context = context with
        {
            SortedItems = Option<IEnumerable<T>>.Some(sortedItems),
            SortingMetadata = Option<IReadOnlyList<SortingMetadata>>.Some(sortingMetadata),
        };

        return await next.Process(context).ConfigureAwait(false);
    }
}
