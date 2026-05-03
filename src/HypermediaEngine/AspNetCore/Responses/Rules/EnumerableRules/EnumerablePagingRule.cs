using HypermediaEngine.Abstractions;
using HypermediaEngine.Requests.Paging;

using LanguageExt;

namespace HypermediaEngine.Responses.Rules.EnumerableRules;

internal sealed class EnumerablePagingRule<T>(ICollectionResponsePipeline<T> next)
    : ICollectionResponsePipeline<T>
    where T : notnull
{
    public async ValueTask<CollectionResponseContext<T>> Process(CollectionResponseContext<T> context)
    {
        IEnumerable<T> filteredItems = context.FilteredItems.Match(
            Some: items => items,
            None: () => context.Items);
        OffsetOrCursorPaging offsetOrCursorPaging = context.QueryParams.Match(
            Some: queryParams => OffsetOrCursorPaging.GetOrDefault(queryParams.Paging),
            None: () => OffsetOrCursorPaging.Default);
        IEnumerable<T> pagedItems = offsetOrCursorPaging.Match(
                                        offset => filteredItems
                                                    .Skip((offset.Page - 1) * offset.PageSize)
                                                    .Take(offset.PageSize),
                                        cursor => filteredItems
                                                    .WhereDynamic(
                                                        "{Field} > {Cursor}",
                                                        new { cursor.Field, cursor.Cursor })
                                                    .Take(cursor.Limit + 1));

        context = context with
        {
            PagedItems = Option<IEnumerable<T>>.Some(pagedItems),
        };

        return await next.Process(context).ConfigureAwait(false);
    }
}
