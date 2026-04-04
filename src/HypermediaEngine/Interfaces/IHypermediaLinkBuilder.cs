namespace HypermediaEngine.Interfaces;

using HypermediaEngine.Models;

public interface IHypermediaLinkBuilder<T>
{
    IHypermediaLinkBuilder<T> WithData(T data);
    IHypermediaLinkBuilder<T> WithLink(string rel, string href, string method = "GET", string? title = null);
    IHypermediaLinkBuilder<T> WithMetadata(string key, object? value);
    HypermediaResponse<T> Build();
}
