using HypermediaEngine.Abstractions;
using HypermediaEngine.Builders;
using HypermediaEngine.Responses;

namespace HypermediaEngine.Helpers;

/// <summary>
/// Provides extension methods for creating hypermedia responses from individual data items or collections.
/// </summary>
/// <remarks>These extension methods simplify the process of constructing hypermedia responses by allowing
/// developers to configure response builders for both single objects and collections. They are intended to be used with
/// types that represent data to be exposed as hypermedia resources in web APIs.</remarks>
public static class HypermediaHelpers
{
    /// <summary>
    /// Creates a hypermedia response that contains the specified data and applies additional configuration using the
    /// provided builder action.
    /// </summary>
    /// <remarks>Use this method to dynamically construct hypermedia responses by supplying both the data and
    /// a configuration action that customizes the response. This enables flexible addition of links and controls based
    /// on the application's requirements.</remarks>
    /// <typeparam name="T">The type of the data to include in the hypermedia response.</typeparam>
    /// <param name="data">The data to be included in the hypermedia response.</param>
    extension<T>(T data)
        where T : notnull
    {
        /// <summary>
        /// Creates a hypermedia response that encapsulates the specified data and applies additional configuration using
        /// the provided builder action.
        /// </summary>
        /// <remarks>Use this method to dynamically construct hypermedia responses by supplying both the data and
        /// a configuration action that customizes the response. This enables flexible addition of links and controls based
        /// on the application's requirements.</remarks>
        /// <param name="configure">An action that configures the hypermedia builder, allowing the addition of links, metadata, or other hypermedia
        /// controls.</param>
        /// <returns>A <see cref="HypermediaObjectBuilder{T}"/> instance containing the provided data and any configured hypermedia elements.</returns>
        public HypermediaObjectResponse<T> ToHypermediaResponse(
            Action<IHypermediaObjectBuilder<T>>? configure = null)
        {
            IHypermediaObjectBuilder<T> builder = new HypermediaObjectBuilder<T>().WithData(data);
            configure?.Invoke(builder);
            return builder.Build();
        }
    }

    /// <summary>
    /// Provides extension methods for creating hypermedia collection responses from enumerable item sequences.
    /// </summary>
    /// <remarks>Use these extension methods to construct hypermedia responses for collections, particularly
    /// when implementing RESTful APIs that adhere to HATEOAS principles. The available methods allow customization of
    /// the response by adding links, metadata, or other hypermedia controls as needed.</remarks>
    /// <typeparam name="T">The type of the items included in the hypermedia collection response.</typeparam>
    /// <param name="items">The collection of items to include in the hypermedia response. Cannot be null.</param>
    extension<T>(IEnumerable<T> items) where T : notnull
    {
        /// <summary>
        /// Creates a hypermedia collection response from the specified items, allowing customization of the response using
        /// a builder configuration action.
        /// </summary>
        /// <remarks>Use this method to dynamically construct hypermedia responses for collections, such as when
        /// building RESTful APIs that follow HATEOAS principles. The configure action allows you to add links or metadata
        /// to the response as needed.</remarks>
        /// <param name="configure">An action that configures the hypermedia collection builder, enabling customization of links, metadata, or other
        /// hypermedia controls.</param>
        /// <returns>A <see cref="HypermediaCollectionResponse{T}"/> containing the configured items and any additional hypermedia controls
        /// specified by the builder.</returns>
        public HypermediaCollectionResponse<T> ToHypermediaCollectionResponse(
            Action<IHypermediaCollectionBuilder<T>>? configure = null
        )
        {
            IHypermediaCollectionBuilder<T> builder = new HypermediaCollectionBuilder<T>().WithItems(items);
            configure?.Invoke(builder);
            return builder.Build();
        }
    }
}
