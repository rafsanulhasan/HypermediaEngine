using EntityTagCaching.Models;

using HypermediaEngine.Abstractions;
using HypermediaEngine.Helpers;
using HypermediaEngine.Requests;
using HypermediaEngine.Requests.Filtering;
using HypermediaEngine.Requests.Paging;
using HypermediaEngine.Requests.Sorting;
using HypermediaEngine.Responses;

using LanguageExt;

using Marten.Linq;

using Microsoft.EntityFrameworkCore;

using Spectre.Console;

using System.Linq.Dynamic.Core;

namespace HypermediaEngine.Builders;

/// <summary>
/// Provides a builder for constructing hypermedia collection responses that include a list of items, associated links,
/// and optional metadata for use in hypermedia APIs.
/// </summary>
/// <remarks>Use this builder to fluently configure the items, navigational links, and pagination metadata of a
/// hypermedia collection response before generating the final result. This facilitates the creation of responses that
/// conform to hypermedia-driven API standards, supporting discoverability and navigation for clients.</remarks>
/// <typeparam name="T">The type of the items contained in the hypermedia collection.</typeparam>
public sealed class HypermediaCollectionBuilder<T> : IHypermediaCollectionBuilder<T>
    where T : notnull
{
    private IEnumerable<T> _items = [];
    public ListLinkCollection Links { get; private set; } = new();
    public ListResponseMetadata? Metadata { get; private set; }

    /// <inheritdoc />
    public IHypermediaCollectionBuilder<T> WithItems(IEnumerable<T> items, QueryParams? query = null)
    {
        query ??= new QueryParams();
        Metadata ??= new ListResponseMetadata(EntityTag.Empty);
        IEnumerable<T> filteredItems = query is { Body.Filtering: FilterNode node }
                                     ? items.WhereDynamic(node.ToString())
                                     : items;
        filteredItems = query?.Body?.Sorting is { Count: > 0 } sort
                      ? filteredItems.OrderByDynamic(string.Join(", ", sort.Select(s => s.ToString())))
                      : filteredItems;
        int totalCount = filteredItems.Count();
        _items = query is { Paging: OffsetOrCursorPaging ocp }
               ? [.. ocp.Match(
                    offset => offset is null
                            ? filteredItems.Skip(0).Take(10)
                            : filteredItems.Skip((offset.Page - 1) * offset.PageSize).Take(offset.PageSize),
                    cursor => cursor is null || string.IsNullOrWhiteSpace(cursor.Cursor)
                            ? filteredItems.Take(cursor?.Limit ?? 10)
                            : filteredItems.WhereDynamic($"Id > {cursor.Cursor}").Take(cursor.Limit)),
               ]
               : [.. filteredItems];

        int count = query?.Paging?.Match(
                    paging => paging is null ? 10 : paging.PageSize,
                    paging => paging is null ? 10 : paging.Limit)
                 ?? _items.Count();
        GetPagingMetadata(totalCount, count, query);
        return this;
    }

    /// <inheritdoc />
    public async Task<IHypermediaCollectionBuilder<T>> WithItemsAsync(
        IMartenQueryable<T> martenQuery,
        QueryParams queryParams,
        CancellationToken ct)
    {
        Metadata ??= new ListResponseMetadata(EntityTag.Empty);
        IQueryable<T> query = martenQuery.ApplyFiltering(queryParams.Body?.Filtering);
        int totalCount = await query.CountAsync(ct).ConfigureAwait(false);
        query = query.ApplySorting(queryParams.Body?.Sorting)
                     .ApplyPaging(queryParams.Paging);
        _items = await query.ToListAsync(ct).ConfigureAwait(false);
        int count = _items switch
        {
            ICollection<T> list => list.Count,
            _ => _items!.Count(),
        };
        Metadata = Metadata with
        {
            Paging = GetPagingMetadata(totalCount, count, queryParams),
        };
        return this;
    }

    /// <inheritdoc />
    public async Task<IHypermediaCollectionBuilder<T>> WithItemsAsync(
        IQueryable<T> query,
        QueryParams queryParams,
        CancellationToken ct)
    {
        Metadata ??= new ListResponseMetadata(EntityTag.Empty);
        query = query.ApplyFiltering(queryParams.Body?.Filtering);
        int totalCount = await query.CountAsync(ct).ConfigureAwait(false);
        query = query.ApplySorting(queryParams.Body?.Sorting)
                     .ApplyPaging(queryParams.Paging);
        _items = await query.ToListAsync(ct).ConfigureAwait(false);
        int count = _items switch
        {
            ICollection<T> list => list.Count,
            _ => _items!.Count(),
        };
        Metadata = Metadata with
        {
            Paging = GetPagingMetadata(totalCount, count, queryParams),
        };
        return this;
    }

    /// <inheritdoc />
    public IHypermediaCollectionBuilder<T> WithSelfLink(string href, string method = "GET", string? title = null)
    {
        HypermediaLink link = new(href, method, LinkRelations.Self, $"ListOf{typeof(T).Name}", title);
        return WithSelfLink(link);
    }

    /// <inheritdoc />
    public IHypermediaCollectionBuilder<T> WithSelfLink(HypermediaLink link)
    {
        ArgumentNullException.ThrowIfNull(link);
        ArgumentException.ThrowIfNullOrWhiteSpace(link.Relationship, nameof(link));

        Links.Self = link;
        return this;
    }

    public IHypermediaCollectionBuilder<T> WithStateTransitionLink(
        LinkRelations rel,
        string href,
        string method = "GET",
        string? type = null,
        string? title = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(href);
        ArgumentException.ThrowIfNullOrEmpty(method);

        HypermediaLink link = new(href, method, rel, type, title);
        return WithStateTransitionLink(link);
    }

    public IHypermediaCollectionBuilder<T> WithStateTransitionLink(HypermediaLink link)
    {
        ArgumentNullException.ThrowIfNull(link);

        Links.StateTransitions ??= [];
        if (link is { Relationship: not null })
        {
            Links.StateTransitions[link.Relationship] = link;
        }
        return this;
    }

    public IHypermediaCollectionBuilder<T> WithRelatedLink(
        LinkRelations rel,
        string href,
        string method = "GET",
        string? type = null,
        string? title = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(href);
        ArgumentException.ThrowIfNullOrEmpty(method);

        HypermediaLink link = new HypermediaLink(href, method, rel, type, title);
        return WithRelatedLink(link);
    }

    public IHypermediaCollectionBuilder<T> WithRelatedLink(HypermediaLink link)
    {
        ArgumentNullException.ThrowIfNull(link);

        Links.Related ??= [];
        if (link is { Relationship: not null })
        {
            Links.Related[link.Relationship] = link;
        }
        return this;
    }

    public IHypermediaCollectionBuilder<T> WithPageLink(
        string href,
        LinkRelations rel,
        string method = "GET",
        string? type = null,
        string? title = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(href);
        ArgumentException.ThrowIfNullOrEmpty(method);

        HypermediaLink link = new(href, method, rel, type, title);
        return WithPageLink(link);
    }

    public IHypermediaCollectionBuilder<T> WithPageLink(HypermediaLink link)
    {
        ArgumentNullException.ThrowIfNull(link);

        Links.Paging ??= [];
        if (!string.IsNullOrWhiteSpace(link.Title))
        {
            Links.Paging[link.Title!] = link;
        }
        return this;
    }

    /// <inheritdoc />
    public HypermediaCollectionResponse<T> Build()
    {
        return new HypermediaCollectionResponse<T>(_items, _items.Count())
        {
            Links = Links,
            Meta = Metadata,
        };
    }

    public IHypermediaCollectionBuilder<T> WithMetadata(ListResponseMetadata metadata)
    {
        Metadata ??= new ListResponseMetadata(EntityTag.Empty);
        Metadata = Metadata with
        {
            ApiVersion = metadata.ApiVersion,
            Domain = metadata.Domain,
            EntityTag = metadata.EntityTag,
            Filters = metadata.Filters,
            Paging = metadata.Paging,
            Sorting = metadata.Sorting,
        };
        return this;
    }

    public IHypermediaCollectionBuilder<T> WithQueryParams(QueryParams queryParams)
    {
        Metadata ??= new ListResponseMetadata(EntityTag.Empty);
        Metadata = Metadata with
        {
            Filters = Metadata.Filters ?? queryParams.Body?.Filtering,
            Sorting = Metadata.Sorting
                   ?? queryParams.Body?.Sorting?.Select(sort
                        => new SortingMetadata(sort.Field, sort.Direction)).ToList().AsReadOnly()
                   ?? [],
            Paging = Metadata.Paging
                  ?? (queryParams.Paging is null
                     ? new PagingMetadata()
                     {
                         CurrentPage = 1,
                         PageSize = 10,
                         Style = PagingStyles.Offset,
                     }
                     : queryParams.Paging.Match(
                           GetPagingMetadata,
                           GetPagingMetadata)),
        };
        return this;
    }

    private PagingMetadata GetPagingMetadata(int totalCount, int count, QueryParams? queryParams)
    {
        Metadata ??= new ListResponseMetadata(EntityTag.Empty);
        PagingMetadata? pagingMetadataFromQuery = queryParams?.Paging?.Match(
                                                    GetPagingMetadata,
                                                    GetPagingMetadata)
                                              ?? Metadata.Paging;
        int? totalPages = pagingMetadataFromQuery?.Style == PagingStyles.Offset
                        ? (int)Math.Ceiling(totalCount / (double)count)
                        : null;
        (PagingStyles PagingStyle, bool HasNext, bool? HasPrevious) = queryParams?.Paging?.Match(
                                        paging => (PagingStyles.Offset, (paging?.Page ?? 1) < totalPages, (paging?.Page ?? 1) > 1),
                                        paging => (PagingStyles.Cursor, string.IsNullOrWhiteSpace(paging.Cursor), default))
                                ?? (PagingStyles.Offset, 1 < totalPages, false);
        pagingMetadataFromQuery = pagingMetadataFromQuery is null
                                ? new PagingMetadata()
                                {
                                    TotalCount = totalCount,
                                    TotalPages = totalPages,
                                    PageSize = count,
                                    HasNext = HasNext,
                                    HasPrevious = HasPrevious,
                                    Style = PagingStyle,
                                }
                                : pagingMetadataFromQuery with
                                {
                                    TotalCount = totalCount,
                                    TotalPages = totalPages,
                                    PageSize = count,
                                    HasNext = HasNext,
                                    HasPrevious = HasPrevious,
                                    Style = PagingStyle,
                                };
        Metadata = Metadata with { Paging = pagingMetadataFromQuery };
        return Metadata.Paging;
    }

    private PagingMetadata GetPagingMetadata(OffsetPaging paging)
    {
        Metadata ??= new ListResponseMetadata(EntityTag.Empty);
        PagingStyles pagingStyle = PagingStyles.Offset;
        PagingMetadata meta = Metadata.Paging is null
                              ? new PagingMetadata()
                              {
                                  CurrentPage = paging?.Page ?? 1,
                                  PageSize = paging?.PageSize ?? 10,
                                  Style = pagingStyle,
                              }
                              : Metadata.Paging with
                              {
                                  CurrentPage = paging?.Page ?? 1,
                                  PageSize = paging?.PageSize ?? 10,
                                  Style = pagingStyle,
                              };
        Metadata = Metadata with { Paging = meta };
        return Metadata.Paging;
    }

    private PagingMetadata GetPagingMetadata(CursorPaging paging)
    {
        Metadata ??= new ListResponseMetadata(EntityTag.Empty);
        PagingStyles pagingStyle = PagingStyles.Cursor;
        PagingMetadata meta = Metadata.Paging is null
                              ? new PagingMetadata()
                              {
                                  Cursor = paging?.Cursor,
                                  PageSize = paging?.Limit ?? 10,
                                  Style = pagingStyle,
                              }
                              : Metadata.Paging with
                              {
                                  Cursor = paging.Cursor,
                                  PageSize = paging.Limit,
                                  Style = pagingStyle,
                              };
        Metadata = Metadata with { Paging = meta };
        return Metadata.Paging;
    }
}
