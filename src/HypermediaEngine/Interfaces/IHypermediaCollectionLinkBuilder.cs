namespace HypermediaEngine.Interfaces;

using HypermediaEngine.Models;

public interface IHypermediaCollectionLinkBuilder<T>
{
    IHypermediaCollectionLinkBuilder<T> WithItems(IEnumerable<T> items);
    IHypermediaCollectionLinkBuilder<T> WithLink(string rel, string href, string method = "GET", string? title = null);
    IHypermediaCollectionLinkBuilder<T> WithMetadata(int totalCount, int page, int pageSize);
    HypermediaCollectionResponse<T> Build();
}
