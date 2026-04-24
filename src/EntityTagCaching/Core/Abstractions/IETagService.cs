namespace EntityTagCaching.Abstractions;

/// <summary>
/// Defines methods for generating and validating entity tags (ETags) used for web resource caching and conditional HTTP
/// requests.
/// </summary>
/// <remarks>Implementations of this interface enable support for ETag-based cache validation, allowing clients
/// and servers to efficiently determine whether a resource has changed. ETags are commonly used in HTTP headers to
/// optimize bandwidth and improve application performance.</remarks>
public interface IETagService
{
    /// <summary>
    /// Generates an entity tag (ETag) that represents the current state of the specified resource for use in HTTP
    /// caching and validation.
    /// </summary>
    /// <remarks>The generated ETag can be used by clients and servers to determine whether a resource has
    /// changed between requests. The ETag value is typically a hash or unique identifier derived from the resource's
    /// state. Supplying a different resource instance or modifying the resource will result in a different
    /// ETag.</remarks>
    /// <param name="resource">The resource object for which to generate the ETag. This object should accurately reflect the current state of
    /// the resource. Cannot be null.</param>
    /// <returns>A string containing the generated ETag that uniquely identifies the state of the resource.</returns>
    string GenerateETag(object resource);

    /// <summary>
    /// Determines whether the specified ETag value is considered stale based on the provided If-None-Match header
    /// value.
    /// </summary>
    /// <remarks>Use this method to validate cache freshness in HTTP scenarios. If the ETag matches any value
    /// in the If-None-Match header, the resource is considered fresh and does not require re-fetching.</remarks>
    /// <param name="etag">The ETag value to evaluate for staleness. Cannot be null.</param>
    /// <param name="ifNoneMatch">The value of the If-None-Match HTTP header, which may contain one or more ETag values to compare against. Can be
    /// null.</param>
    /// <returns>true if the ETag is stale and does not match any value in the If-None-Match header; otherwise, false.</returns>
    bool IsETagStale(string etag, string? ifNoneMatch);
}

