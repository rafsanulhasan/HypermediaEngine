namespace HypermediaEngine.Interfaces;

using HypermediaEngine.Models;

public interface IHypermediaService
{
    HypermediaResponse<T> CreateResponse<T>(T data, Action<IHypermediaLinkBuilder<T>> configureLinks);
    HypermediaCollectionResponse<T> CreateCollectionResponse<T>(IEnumerable<T> items, Action<IHypermediaCollectionLinkBuilder<T>> configureLinks);
}
