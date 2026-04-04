namespace HypermediaEngine.Extensions;

using HypermediaEngine.Builders;
using HypermediaEngine.Models;

public static class HypermediaExtensions
{
    public static HypermediaResponse<T> ToHypermediaResponse<T>(
        this T data,
        Action<HypermediaBuilder<T>> configure)
    {
        var builder = new HypermediaBuilder<T>().WithData(data);
        configure(builder);
        return builder.Build();
    }

    public static HypermediaCollectionResponse<T> ToHypermediaCollectionResponse<T>(
        this IEnumerable<T> items,
        Action<HypermediaCollectionBuilder<T>> configure)
    {
        var builder = new HypermediaCollectionBuilder<T>().WithItems(items);
        configure(builder);
        return builder.Build();
    }
}
