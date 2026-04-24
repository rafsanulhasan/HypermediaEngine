using HypermediaEngine.Requests;
using HypermediaEngine.Responses;

namespace HypermediaEngine.Abstractions;

/// <summary>
/// Defines a builder for constructing hypermedia collection responses, allowing the addition of items, links, and
/// metadata in a fluent manner.
/// </summary>
/// <remarks>Use this interface to incrementally build a hypermedia response that represents a collection of
/// resources, including associated links and pagination metadata. Implementations should ensure that the resulting
/// response conforms to hypermedia standards and is suitable for use in RESTful APIs.</remarks>
/// <typeparam name="T">The type of items contained in the hypermedia collection.</typeparam>
public interface IHypermediaCollectionBuilder<T> : IHypermediaBuilder<T>
    where T : notnull
{
    ListLinkCollection Links { get; }
    ListResponseMetadata? Metadata { get; }

    /// <summary>
    /// Configures the collection link builder to include the specified items in the hypermedia response.
    /// </summary>
    /// <remarks>Use this method to specify the items that will be serialized and returned as part of the
    /// hypermedia collection. Ensure that the provided items are properly initialized and meet any criteria required
    /// for inclusion in the response.</remarks>
    /// <param name="items">The collection of items to be included in the hypermedia response. This parameter cannot be null and must
    /// contain valid items of type T.</param>
    /// <returns>An instance of <see cref="IHypermediaCollectionBuilder{T}"/> that can be used for further configuration of the hypermedia
    /// response.</returns>
    IHypermediaCollectionBuilder<T> WithItems(IEnumerable<T> items, QueryParams? query = null);

    /// <summary>
    /// Configures the collection link builder to total filtered items in the hypermedia response.
    /// </summary>
    /// <remarks>Use this method to populate the number of filtered items that will be serialized and returned as part of the
    /// hypermedia collection paging metadata.</remarks>
    /// <param name="query">The collection of items to be included in the hypermedia response. This parameter cannot be null and must
    /// contain valid items of type T.</param>
    /// <returns>An instance of <see cref="IHypermediaCollectionBuilder{T}"/> that can be used for further configuration of the hypermedia
    /// response.</returns>
    Task<IHypermediaCollectionBuilder<T>> WithItemsAsync(
        IQueryable<T> filteredQuery,
        QueryParams queryParams,
        CancellationToken ct);

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
    IHypermediaCollectionBuilder<T> WithSelfLink(string href, string method = "GET", string? title = null);
    IHypermediaCollectionBuilder<T> WithSelfLink(HypermediaLink link);
    IHypermediaCollectionBuilder<T> WithStateTransitionLink(
        LinkRelations rel,
        string href,
        string method = "GET",
        string? type = null,
        string? title = null);
    IHypermediaCollectionBuilder<T> WithStateTransitionLink(HypermediaLink link);
    IHypermediaCollectionBuilder<T> WithRelatedLink(
        LinkRelations rel,
        string href,
        string method = "GET",
        string? type = null,
        string? title = null);
    IHypermediaCollectionBuilder<T> WithRelatedLink(HypermediaLink link);

    IHypermediaCollectionBuilder<T> WithPageLink(
        string href,
        LinkRelations rel,
        string method = "GET",
        string? type = null,
        string? title = null);
    IHypermediaCollectionBuilder<T> WithPageLink(HypermediaLink link);
    IHypermediaCollectionBuilder<T> WithMetadata(ListResponseMetadata metadata);

    /// <summary>
    /// Builds and returns a hypermedia collection response containing the specified type.
    /// </summary>
    /// <remarks>This method is typically used to construct a response that includes hypermedia links and
    /// related data for the specified type. Ensure that the necessary data is populated before calling this
    /// method.</remarks>
    /// <returns>A <see cref="HypermediaCollectionResponse{T}"/> that encapsulates the hypermedia collection response data.</returns>
    HypermediaCollectionResponse<T> Build();
}
