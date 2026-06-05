using LanguageExt;

using SynergyFx.HypermediaEngine.Abstractions;
using SynergyFx.HypermediaEngine.Requests.Paging;
using SynergyFx.HypermediaEngine.Responses.Metadata;

namespace SynergyFx.HypermediaEngine.Responses.Rules.QueryableRules;

internal sealed class QueryableFinalPagingRule<T>
    : ICollectionResponsePipeline<T>
    where T : notnull
{
    public ValueTask<CollectionResponseContext<T>> Process(CollectionResponseContext<T> context)
    {
        IQueryable<T> sortedQuery = context.SortedQuery.Match(
            Some: q => q,
            None: () => context.Query.Match(
                Some: q => q,
                None: () => Enumerable.Empty<T>().AsQueryable()));
        (IReadOnlyList<T> finalItems, int? currentPage, int pageSize, int? totalPages, bool hasNext, bool hasPrevious, string? requestedCursor, string? cursorField, PagingStyles style) =
            Populate(sortedQuery, context);
        PagingMetadata pagingMetadata = Populate(
            context,
            currentPage,
            pageSize,
            totalPages,
            hasNext,
            hasPrevious,
            requestedCursor,
            cursorField,
            style);

        context = context with
        {
            FinalItems = Option<IReadOnlyList<T>>.Some(finalItems),
            PagingMetadata = pagingMetadata,
        };

        return ValueTask.FromResult(context);
    }

    private static (IReadOnlyList<T> finalItems, int? currentPage, int pageSize, int? totalPages, bool hasNext, bool hasPrevious, string? requestedCursor, string? cursorField, PagingStyles style) Populate(
        IQueryable<T> sortedQuery,
        CollectionResponseContext<T> context
    )
    {
        OffsetOrCursorPaging offsetOrCursorPaging = context.QueryParams.Match(
            Some: queryParams => OffsetOrCursorPaging.GetOrDefault(queryParams.Paging),
            None: () => OffsetOrCursorPaging.Default);
        IEnumerable<T> pagedItemsStream = offsetOrCursorPaging.Match(
                                            _ => sortedQuery,
                                            _ => sortedQuery.AsEnumerable().SkipLast());
        IReadOnlyList<T> pagedItemsList = [.. pagedItemsStream];
        int pagedItemCount = pagedItemsList.Count;
        int? totalPages = context.PagingMetadata.Match(
            Some: paging => (int?)Math.Ceiling((decimal)paging.TotalCount / pagedItemCount),
            None: () => null);
        return offsetOrCursorPaging.Match(
            f0: offset => (
                pagedItemsList,
                currentPage: offset.Page,
                pageSize: pagedItemCount,
                totalPages,
                hasNext: offset.Page < totalPages,
                hasPrevious: offset.Page > 1,
                requestedCursor: null,
                cursorField: null,
                style: PagingStyles.Offset
            ),
            f1: cursor => (
                pagedItemsList,
                currentPage: (int?)null,
                pageSize: pagedItemCount,
                totalPages: (int?)null,
                hasNext: pagedItemCount > 0,
                hasPrevious: string.IsNullOrWhiteSpace(cursor.Cursor).Equals(false),
                requestedCursor: cursor.Cursor,
                cursorField: cursor.Field,
                style: PagingStyles.Cursor
            )
        );
    }

    private static PagingMetadata Populate(
        CollectionResponseContext<T> context,
        int? currentPage,
        int pageSize,
        int? totalPages,
        bool hasNext,
        bool hasPrevious,
        string? requestedCursor,
        string? cursorField,
        PagingStyles style)
    {
        PagingMetadata pagingMetadata = context.PagingMetadata.Match(
            Some: paging => paging with
            {
                CurrentPage = currentPage,
                PageSize = pageSize,
                TotalPages = totalPages,
                HasNext = hasNext,
                HasPrevious = hasPrevious,
                RequestedCursor = requestedCursor,
                CursorField = cursorField,
                Style = style,
            },
            None: () => new()
            {
                CurrentPage = currentPage,
                PageSize = pageSize,
                TotalPages = totalPages,
                HasNext = hasNext,
                HasPrevious = hasPrevious,
                RequestedCursor = requestedCursor,
                CursorField = cursorField,
                Style = style,
            });
        return pagingMetadata;
    }
}
