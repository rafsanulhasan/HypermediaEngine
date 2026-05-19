using HypermediaEngine.Abstractions;
using HypermediaEngine.Requests.Paging;

using LanguageExt;

namespace HypermediaEngine.Responses.Rules.QueryableRules;

internal sealed class QueryablePagingRule<T>(ICollectionResponsePipeline<T> next)
    : ICollectionResponsePipeline<T>
    where T : notnull
{
    public async ValueTask<CollectionResponseContext<T>> Process(CollectionResponseContext<T> context)
    {
        IQueryable<T> source = context.FilteredQuery.Match(
            Some: q => q,
            None: () => context.Query.Match(
                Some: q => q,
                None: () => Enumerable.Empty<T>().AsQueryable()));
        OffsetOrCursorPaging offsetOrCursorPaging = context.QueryParams.Match(
            Some: queryParams => OffsetOrCursorPaging.GetOrDefault(queryParams.Paging),
            None: () => OffsetOrCursorPaging.Default);
        IQueryable<T> pagedQuery = offsetOrCursorPaging.Match(
            offset => source
                        .Skip((offset.Page - 1) * offset.PageSize)
                        .Take(offset.PageSize),
            cursor => source
                        .WhereDynamic(
                            "{Field} > {Cursor}",
                            new { cursor.Field, cursor.Cursor })
                        .Take(cursor.Limit + 1));

        context = context with
        {
            PagedQuery = Option<IQueryable<T>>.Some(pagedQuery),
        };

        return await next.Process(context).ConfigureAwait(false);
    }
}
