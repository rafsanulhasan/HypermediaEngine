
using HypermediaEngine.Abstractions;
using HypermediaEngine.Builders;
using HypermediaEngine.Responses;

namespace HypermediaEngine.Services;

/// <summary>
/// Provides methods for creating hypermedia responses and collections, enabling the inclusion of links and metadata in
/// API responses according to hypermedia principles.
/// </summary>
/// <remarks>The HypermediaService simplifies the construction of hypermedia-compliant responses by using builder
/// patterns. It supports both single resource and collection responses, allowing developers to attach relevant links
/// and metadata to the returned data. This service is typically used in web APIs that follow HATEOAS or similar
/// hypermedia-driven designs.</remarks>
public class HypermediaService : IHypermediaService
{
    /// <summary>
    /// Creates a hypermedia response that includes the specified data and optionally configures additional hypermedia
    /// links.
    /// </summary>
    /// <remarks>Use this method to dynamically generate hypermedia responses, allowing for the inclusion of
    /// custom links based on the provided configuration action.</remarks>
    /// <typeparam name="T">The type of the data to include in the hypermedia response. Must be a non-nullable type.</typeparam>
    /// <param name="data">The data to include in the hypermedia response. This parameter cannot be null.</param>
    /// <param name="configureLinks">An optional action to configure additional hypermedia links for the response. If provided, this action receives
    /// an instance of the hypermedia builder to customize the links.</param>
    /// <returns>A hypermedia response containing the provided data and any configured links.</returns>
    public HypermediaObjectResponse<T> CreateResponse<T>(T data, Action<IHypermediaBuilder<T>>? configureLinks = null)
        where T : notnull
    {
        HypermediaObjectBuilder<T> builder = new();
        builder.WithData(data);
        configureLinks?.Invoke(builder);
        return builder.Build();
    }

    /// <summary>
    /// Creates a hypermedia collection response that contains the specified items and optional link configurations.
    /// </summary>
    /// <remarks>Use this method to generate a hypermedia response for a collection of items, with the ability
    /// to add custom links as needed. This facilitates building responses that adhere to HATEOAS principles in RESTful
    /// APIs.</remarks>
    /// <typeparam name="T">The type of the items in the collection. This type must be non-nullable.</typeparam>
    /// <param name="items">The collection of items to include in the hypermedia response. Cannot be null.</param>
    /// <param name="configureLinks">An optional action to configure additional links for the hypermedia collection. If provided, this action is
    /// invoked to customize the response's links.</param>
    /// <returns>A <see cref="HypermediaCollectionResponse{T}"/> that encapsulates the provided items and any configured links.</returns>
    public HypermediaCollectionResponse<T> CreateCollectionResponse<T>(
        IEnumerable<T> items,
        Action<IHypermediaCollectionBuilder<T>>? configureLinks = null
    ) where T : notnull
    {
        HypermediaCollectionBuilder<T> builder = new();
        builder.WithItems(items);
        configureLinks?.Invoke(builder);
        return builder.Build();
    }
}
