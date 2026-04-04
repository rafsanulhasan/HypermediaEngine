namespace HypermediaEngine.Builders;

using HypermediaEngine.Interfaces;
using HypermediaEngine.Models;

public class HypermediaBuilder<T> : IHypermediaLinkBuilder<T>
{
    private T? _data;
    private readonly Dictionary<string, HypermediaLink> _links = new();
    private readonly Dictionary<string, object?> _metadata = new();

    public HypermediaBuilder<T> WithData(T data)
    {
        _data = data;
        return this;
    }

    IHypermediaLinkBuilder<T> IHypermediaLinkBuilder<T>.WithData(T data) => WithData(data);

    public HypermediaBuilder<T> WithLink(string rel, string href, string method = "GET", string? title = null)
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

    IHypermediaLinkBuilder<T> IHypermediaLinkBuilder<T>.WithLink(string rel, string href, string method, string? title) => WithLink(rel, href, method, title);

    public HypermediaBuilder<T> WithMetadata(string key, object? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        _metadata[key] = value;
        return this;
    }

    IHypermediaLinkBuilder<T> IHypermediaLinkBuilder<T>.WithMetadata(string key, object? value) => WithMetadata(key, value);

    public HypermediaResponse<T> Build()
    {
        return new HypermediaResponse<T>
        {
            Data = _data,
            Links = new Dictionary<string, HypermediaLink>(_links),
            Metadata = _metadata.Count > 0 ? new Dictionary<string, object?>(_metadata) : null
        };
    }
}
