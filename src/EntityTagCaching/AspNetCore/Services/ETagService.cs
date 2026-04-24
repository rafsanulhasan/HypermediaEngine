using EntityTagCaching.Abstractions;

using Microsoft.Extensions.DependencyInjection;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace EntityTagCaching.Services;

/// <summary>
/// Represents a method that computes a hash value for the specified byte array, typically used for generating ETag
/// values.
/// </summary>
/// <remarks>Use this delegate to specify a custom hashing algorithm for ETag generation. The implementation
/// should ensure that the returned hash uniquely represents the input data for reliable ETag comparisons.</remarks>
/// <param name="data">The input byte array containing the data to be hashed. Cannot be null.</param>
/// <returns>A byte array containing the computed hash of the input data.</returns>
public delegate byte[] ETagHasher(byte[] data);

/// <summary>
/// Represents a method that asynchronously computes a hash value for the specified byte array.
/// </summary>
/// <remarks>This delegate is intended for scenarios where hashing may involve large data sets or require
/// non-blocking execution. The operation can be canceled by signaling the provided cancellation token.</remarks>
/// <param name="data">The byte array containing the data to be hashed. Cannot be null.</param>
/// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous hash computation.</param>
/// <returns>A task that represents the asynchronous operation. The task result contains a byte array with the computed hash
/// value.</returns>
public delegate ValueTask<int> ETagAsyncHasher(Stream s, Memory<byte> destination, CancellationToken cancellationToken);

/// <summary>
/// Provides functionality for generating and validating ETags for resources to support HTTP caching and conditional
/// requests.
/// </summary>
/// <remarks>The ETagService serializes resources to JSON and computes a SHA-256 hash to generate strong ETags. It
/// also offers methods to determine whether a provided ETag is considered stale based on the If-None-Match header
/// value. This service is typically used in web APIs to optimize bandwidth and improve client-side caching by enabling
/// efficient change detection for resources.</remarks>
public class ETagService : IETagService
{
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ETagAsyncHasher? _asyncHasher;
    private readonly ETagHasher? _hasher;

    /// <summary>
    /// Initializes a new instance of the ETagService class using the specified service provider to configure JSON
    /// serialization options and hashing services.
    /// </summary>
    /// <remarks>If a JsonSerializerOptions service is not registered, default options are created with
    /// indented formatting disabled and camel case property naming policy applied. The service provider is also used to
    /// obtain optional ETag hashing services.</remarks>
    /// <param name="serviceProvider">The service provider used to resolve dependencies required for JSON serialization and ETag hashing
    /// functionality. Cannot be null.</param>
    public ETagService(IServiceProvider serviceProvider)
    {
        _jsonOptions = serviceProvider.GetService<JsonSerializerOptions>()
                    ?? new JsonSerializerOptions
                    {
                        WriteIndented = false,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };
        _asyncHasher = serviceProvider.GetService<ETagAsyncHasher>();
        _hasher = serviceProvider.GetService<ETagHasher>();
    }

    /// <inheritdoc />
    public string GenerateETag(object resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        string json = JsonSerializer.Serialize(resource, _jsonOptions);
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        byte[] hash = SHA256.HashData(bytes);
        string hexSteing = Convert.ToHexString(hash);
        return $"\"{hexSteing[..16]}\"";
    }

    /// <inheritdoc />
    public bool IsETagStale(string etag, string? ifNoneMatch)
    {
        return string.IsNullOrEmpty(ifNoneMatch)
            || !etag.Equals(ifNoneMatch, StringComparison.Ordinal);
    }
}
