using HypermediaEngine.Responses;

namespace HypermediaEngine.Abstractions;

/// <summary>
/// Defines methods for creating hypermedia responses and collections in a hypermedia-driven API.
/// </summary>
/// <remarks>This interface provides functionality to generate responses that include hypermedia links, allowing
/// clients to navigate the API dynamically. Implementations should ensure that the generated responses conform to
/// hypermedia standards.</remarks>
public interface IHypermediaService
{
    /// <summary>
    /// Creates a hypermedia response that encapsulates the specified data and optional hypermedia links.
    /// </summary>
    /// <remarks>Use this method to generate responses for RESTful APIs that follow hypermedia principles,
    /// enabling clients to discover available actions and navigate the API dynamically.</remarks>
    /// <typeparam name="T">The type of the data to include in the hypermedia response.</typeparam>
    /// <param name="data">The data to be included as the primary content of the hypermedia response.</param>
    /// <param name="configureLinks">An optional action to configure additional hypermedia links related to the data. If provided, this action is
    /// invoked to customize the links in the response.</param>
    /// <returns>A <see cref="HypermediaObjectResponse{T}"/> that contains the provided data and any configured hypermedia links.</returns>
    HypermediaObjectResponse<T> CreateResponse<T>(T data, Action<IHypermediaBuilder<T>>? configureLinks = null)
        where T : notnull;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="configureLinks"></param>
    /// <returns></returns>
    HypermediaCollectionResponse<T> CreateCollectionResponse<T>(IEnumerable<T> items, Action<IHypermediaCollectionBuilder<T>>? configureLinks = null)
        where T : notnull;
}
