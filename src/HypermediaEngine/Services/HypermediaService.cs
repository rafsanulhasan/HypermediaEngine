namespace HypermediaEngine.Services;

using HypermediaEngine.Builders;
using HypermediaEngine.Interfaces;
using HypermediaEngine.Models;

public class HypermediaService : IHypermediaService
{
    public HypermediaResponse<T> CreateResponse<T>(T data, Action<IHypermediaLinkBuilder<T>> configureLinks)
    {
        var builder = new HypermediaBuilder<T>();
        builder.WithData(data);
        configureLinks(builder);
        return builder.Build();
    }

    public HypermediaCollectionResponse<T> CreateCollectionResponse<T>(
        IEnumerable<T> items,
        Action<IHypermediaCollectionLinkBuilder<T>> configureLinks)
    {
        var builder = new HypermediaCollectionBuilder<T>();
        builder.WithItems(items);
        configureLinks(builder);
        return builder.Build();
    }
}
