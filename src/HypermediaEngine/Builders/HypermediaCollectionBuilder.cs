namespace HypermediaEngine.Builders;

using HypermediaEngine.Interfaces;
using HypermediaEngine.Models;

public class HypermediaCollectionBuilder<T> : IHypermediaCollectionLinkBuilder<T>
{
    private IEnumerable<T> _items = Enumerable.Empty<T>();
    private readonly Dictionary<string, HypermediaLink> _links = new();
    private HypermediaMetadata? _metadata;

    public HypermediaCollectionBuilder<T> WithItems(IEnumerable<T> items)
    {
        _items = items ?? Enumerable.Empty<T>();
        return this;
    }

    IHypermediaCollectionLinkBuilder<T> IHypermediaCollectionLinkBuilder<T>.WithItems(IEnumerable<T> items) => WithItems(items);

    public HypermediaCollectionBuilder<T> WithLink(string rel, string href, string method = "GET", string? title = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(rel);
        ArgumentException.ThrowIfNullOrEmpty(href);

        _links[rel] = new HypermediaLink
        {
            Href = href,
            Method = method,
            Title = title
        };
        return this;
    }

    IHypermediaCollectionLinkBuilder<T> IHypermediaCollectionLinkBuilder<T>.WithLink(string rel, string href, string method, string? title) => WithLink(rel, href, method, title);

    public HypermediaCollectionBuilder<T> WithMetadata(int totalCount, int page, int pageSize)
    {
        _metadata = new HypermediaMetadata
        {
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
        return this;
    }

    IHypermediaCollectionLinkBuilder<T> IHypermediaCollectionLinkBuilder<T>.WithMetadata(int totalCount, int page, int pageSize) => WithMetadata(totalCount, page, pageSize);

    public HypermediaCollectionResponse<T> Build()
    {
        var itemsList = _items.ToList();
        return new HypermediaCollectionResponse<T>
        {
            Items = itemsList,
            TotalCount = _metadata?.TotalCount ?? itemsList.Count,
            Links = new Dictionary<string, HypermediaLink>(_links),
            Metadata = _metadata
        };
    }
}
