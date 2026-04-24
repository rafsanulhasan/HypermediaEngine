using HypermediaEngine.Responses;

namespace HypermediaEngine.Abstractions;

/// <summary>
/// Provides a fluent interface for constructing hypermedia responses that include data, links, and metadata.
/// </summary>
/// <remarks>Use this interface to build RESTful API responses that follow hypermedia principles, allowing clients
/// to discover available actions and related resources through embedded links and metadata. The builder pattern enables
/// chaining of methods to incrementally add data, links, and metadata before producing a final hypermedia
/// response.</remarks>
/// <typeparam name="T">The type of the data to be included in the hypermedia response.</typeparam>
public interface IHypermediaObjectBuilder<T> : IHypermediaBuilder<T>
    where T : notnull
{
    /// <summary>
    /// Configures the hypermedia builder with the specified data.
    /// </summary>
    /// <remarks>This method allows for the customization of the hypermedia output by including specific data
    /// relevant to the context.</remarks>
    /// <param name="data">The data to associate with the hypermedia builder. This parameter cannot be null.</param>
    /// <returns>An instance of <see cref="IHypermediaBuilder{T}"/> that is configured with the provided data.</returns>
    IHypermediaObjectBuilder<T> WithData(T data);

    /// <summary>
    /// Adds a hypermedia link to the response with the specified relationship, target URL, HTTP method, and optional
    /// title.
    /// </summary>
    /// <remarks>Use this method to add discoverable actions or related resources to a hypermedia-driven API
    /// response. This enables clients to navigate or interact with the API dynamically based on the available
    /// links.</remarks>
    /// Cannot be null or empty.</param>
    /// <param name="href">The target URL of the link. Must be a valid, absolute or relative URI.</param>
    /// <param name="method">The HTTP method to associate with the link. Defaults to "GET" if not specified.</param>
    /// <param name="title">An optional human-readable title for the link, providing additional context for clients.</param>
    /// <returns>An instance of <see cref="IHypermediaBuilder{T}"/> that can be used to further configure the hypermedia response.</returns>
    IHypermediaObjectBuilder<T> WithSelfLink(string href, string method = "GET", string? title = null);
    IHypermediaObjectBuilder<T> WithSelfLink(HypermediaLink link);
    IHypermediaObjectBuilder<T> WithStateTransitionLink(LinkRelations rel, string href, string method = "GET", string? title = null);
    IHypermediaObjectBuilder<T> WithStateTransitionLink(HypermediaLink link);
    IHypermediaObjectBuilder<T> WithRelatedLink(LinkRelations rel, string href, string method = "GET", string? title = null);
    IHypermediaObjectBuilder<T> WithRelatedLink(HypermediaLink link);

    /// <summary>
    /// Adds a metadata entry to the hypermedia builder using the specified key and value.
    /// </summary>
    /// <remarks>Use this method to attach additional information to the hypermedia representation, which can
    /// be accessed by API consumers for custom processing or interpretation.</remarks>
    /// <param name="meta">The unique key that identifies the metadata entry. Cannot be null.</param>
    /// <returns>An instance of <see cref="IHypermediaBuilder{T}"/> with the added metadata, enabling method chaining.</returns>
    IHypermediaObjectBuilder<T> WithMetadata(ObjectResponseMetadata meta);

    /// <summary>
    /// Builds and returns a new instance of the hypermedia response for the current resource.
    /// </summary>
    /// <remarks>Use this method to generate a hypermedia response that enables clients to discover available
    /// actions and related resources dynamically, following HATEOAS principles.</remarks>
    /// <returns>A <see cref="HypermediaObjectResponse{T}"/> that encapsulates the hypermedia data and links for the client.</returns>
    HypermediaObjectResponse<T> Build();
}
