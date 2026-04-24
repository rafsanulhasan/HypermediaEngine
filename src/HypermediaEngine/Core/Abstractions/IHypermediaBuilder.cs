namespace HypermediaEngine.Abstractions;

/// <summary>
/// Provides a fluent interface for constructing hypermedia responses that include data, links, and metadata.
/// </summary>
/// <remarks>Use this interface to build RESTful API responses that follow hypermedia principles, allowing clients
/// to discover available actions and related resources through embedded links and metadata. The builder pattern enables
/// chaining of methods to incrementally add data, links, and metadata before producing a final hypermedia
/// response.</remarks>
/// <typeparam name="T">The type of the data to be included in the hypermedia response.</typeparam>
public interface IHypermediaBuilder<T>
    where T : notnull
{
}
