using LanguageExt;

using Marten;

using SynergyFx.HypermediaEngine.Abstractions;
using SynergyFx.HypermediaEngine.Requests.Paging;
using SynergyFx.HypermediaEngine.Responses.Metadata;

namespace SynergyFx.HypermediaEngine.Responses.Rules.MartenQueryableRules;

internal sealed class MartenQueryableFinalPagingRule<T>
    : ICollectionResponsePipeline<T>
    where T : notnull
{
    public async ValueTask<CollectionResponseContext<T>> Process(CollectionResponseContext<T> context)
    {
        IQueryable<T> source = context.SortedQuery.Match(
            Some: q => q,
            None: () => context.Query.Match(
                Some: q => q,
                None: () => throw new InvalidOperationException(
                    "CollectionResponseContext must have SortedQuery or Query set before the MartenQueryable final paging rule.")));

        (IReadOnlyList<T> finalItems, int? currentPage, int pageSize, int? totalPages, bool hasNext, bool hasPrevious, string? requestedCursor, string? cursorField, PagingStyles style) =
            await Populate(source, context).ConfigureAwait(false);

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

        return context;
    }

    private static async ValueTask<(IReadOnlyList<T> finalItems, int? currentPage, int pageSize, int? totalPages, bool hasNext, bool hasPrevious, string? requestedCursor, string? cursorField, PagingStyles style)> Populate(
        IQueryable<T> source,
        CollectionResponseContext<T> context)
    {
        OffsetOrCursorPaging offsetOrCursorPaging = context.QueryParams.Match(
            Some: queryParams => OffsetOrCursorPaging.GetOrDefault(queryParams.Paging),
            None: () => OffsetOrCursorPaging.Default);

        IReadOnlyList<T> materialized = await source.ToListAsync().ConfigureAwait(false);

        IReadOnlyList<T> finalItems = offsetOrCursorPaging.Match(
            _ => materialized,
            _ => (IReadOnlyList<T>)[.. materialized.SkipLast(1)]);

        int pagedItemCount = finalItems.Count;
        int? totalPages = context.PagingMetadata.Match(
            Some: paging => pagedItemCount > 0
                ? (int?)Math.Ceiling((decimal)paging.TotalCount / pagedItemCount)
                : null,
            None: () => null);

        return offsetOrCursorPaging.Match(
            f0: offset => (
                finalItems,
                currentPage: (int?)offset.Page,
                pageSize: pagedItemCount,
                totalPages,
                hasNext: offset.Page < totalPages,
                hasPrevious: offset.Page > 1,
                requestedCursor: (string?)null,
                cursorField: (string?)null,
                style: PagingStyles.Offset
            ),
            f1: cursor => (
                finalItems,
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
