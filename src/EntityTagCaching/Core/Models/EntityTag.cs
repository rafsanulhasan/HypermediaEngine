using EntityTagCaching.Abstractions;

using LanguageExt;
using LanguageExt.Common;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi;

using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EntityTagCaching.Models;

[JsonConverter(typeof(EntityTagJsonConverter))]
[StructLayout(LayoutKind.Auto)]
public readonly record struct EntityTag
    : IEquatable<StringValues>,
      IEquatable<string>,
      IEquatable<Option<EntityTag>>,
      IEquatable<Result<EntityTag>>,
      IEquatable<OptionalResult<EntityTag>>
{
    private EntityTag(StringValues value, EntityTagStrength? strength)
    {
        Value = value;
        Strength = strength;
    }

    public static readonly EntityTag Empty = new(StringValues.Empty, null);

    public StringValues Value { get; }
    public EntityTagStrength? Strength { get; }

    public static OptionalResult<EntityTag> Create<T>(T? data, int size = 256)
    {
        if (data is null)
        {
            return new OptionalResult<EntityTag>(
                new ArgumentNullException(nameof(data)));
        }
        MemoryStream ms = new();
        try
        {
            Span<byte> bytes = stackalloc byte[size];
            Span<byte> hashBytes = stackalloc byte[size];
            int hashBytesWritten;
            if (data is ITaggableEntity taggableEntity)
            {
                _ = taggableEntity.WriteBinary(bytes);
                hashBytesWritten = GetStrongETagBytes(taggableEntity, bytes, hashBytes);
                string strongEtag = GetStrongETag(hashBytes, hashBytesWritten);
                return new EntityTag(strongEtag, EntityTagStrength.Strong);
            }

            hashBytesWritten = GetWeakETagBytes(data, ms, bytes, hashBytes);
            string weakEtag = GetWeakETag(hashBytes, hashBytesWritten);
            return new EntityTag(weakEtag, EntityTagStrength.Weak);
        }
        catch (Exception ex)
        {
            return new OptionalResult<EntityTag>(ex);
        }
        finally
        {
            ms.Dispose();
        }
    }

    public static Option<EntityTag> From(StringValues etagValues)
    {
        string etag = etagValues.ToString();
        if (string.IsNullOrWhiteSpace(etag))
        {
            return Option<EntityTag>.None;
        }

        EntityTagStrength strength = etag.StartsWith("W/", StringComparison.InvariantCulture)
                                   ? EntityTagStrength.Weak
                                   : EntityTagStrength.Strong;

        return new EntityTag(etag, strength);
    }

    public static Option<EntityTag> From(HttpRequest request)
    {
        return request switch
        {
            HttpRequest r when r.Headers.TryGetValue(HeaderNames.IfNoneMatch, out StringValues ifNoneMatch)
                => From(ifNoneMatch),
            HttpRequest r when r.Headers.TryGetValue(HeaderNames.IfMatch, out StringValues ifMatch)
                => From(ifMatch),
            _ => Option<EntityTag>.None,
        };
    }

    internal static IOpenApiParameter CreateOpenApiParameter(string requestMethod)
    {
        OpenApiParameter param = new()
        {
            In = ParameterLocation.Header,
            AllowEmptyValue = false,
            Schema = CreateOpenApiSchema(),
        };
        if (requestMethod.Equals(HttpMethods.Get, StringComparison.OrdinalIgnoreCase))
        {
            param.Name = HeaderNames.IfNoneMatch;
            param.Description = "If provided, makes the request conditional for GET or HEAD. The server will only return the resource (200 OK) if the current ETag does not match any listed; otherwise, it returns 304 Not Modified with the body stripped.";
        }
        else if (requestMethod.Equals(HttpMethods.Put, StringComparison.OrdinalIgnoreCase)
              || requestMethod.Equals(HttpMethods.Patch, StringComparison.OrdinalIgnoreCase))
        {
            param.Name = HeaderNames.IfMatch;
            param.Description = "Primarily used for safe updates ensuring <strong>Last-Write-Wins</strong> protected for PUT or PATCH. The server will only perform the action if the resource's current ETag exactly matches the one provided, preventing \"lost updates\" (412 Precondition Failed).";
        }
        return param;
    }

    internal static OpenApiResponse CreateOpenApiResponse()
    {
        OpenApiResponse newResponse = new()
        {
            Headers = new Dictionary<string, IOpenApiHeader>(StringComparer.OrdinalIgnoreCase)
            {
                {
                    HeaderNames.ETag,
                    new OpenApiHeader()
                    {
                        Description = "The entity tag (ETag) uniquely identifies the resource version and must be enclosed in double quotes. <br/><br/>"
                                    + "**Strong ETags** (Hex string) represent a byte-for-byte identical version of the entity (e.g., \"123-abc\"). <br/>"
                                    + "**Weak ETags** (Base64 string) identify the entity semantically rather than byte-for-byte (e.g., W/\"123-abc\").<br/><br/>"
                                    + "<strong>RFC 9110</strong>:"
                                    + "<ul>"
                                    + "<li>ETags should be enclosed with double qoutes.</li>"
                                    + "<li>Weak ETags should be prefixed with 'W/'.</li>"
                                    + "<li>Strong ETags represent a byte-for-byte identical version of the entity.</li>"
                                    + "<li>Weak ETag identifies the entity semantically rather than byte-for-byte.</li>"
                                    + "</ul>",
                        Schema = new OpenApiSchema
                        {
                            Type = JsonSchemaType.String,
                        },
                    }
                },
            },
        };
        return newResponse;
    }

    internal static IOpenApiSchema CreateOpenApiSchema()
    {
        return new OpenApiSchema
        {
            Type = JsonSchemaType.String,
        };
    }

    internal static IOpenApiResponse ModifyOpenApiresponse(IOpenApiResponse response, OpenApiResponse newResponse)
    {
        newResponse.Content = response.Content;
        newResponse.Description = response.Description;
        newResponse.Extensions = response.Extensions;
        newResponse.Links = response.Links;
        return newResponse;
    }

    public static implicit operator StringValues(EntityTag tag)
    {
        return tag.Value;
    }

    public static bool operator ==(EntityTag etag, string value)
    {
        return etag.Value.Equals(value);
    }

    public static bool operator ==(EntityTag etag, StringValues value)
    {
        return etag.Value.Equals(value);
    }

    public static bool operator ==(EntityTag etag, Option<EntityTag> value)
    {
        return value.Match(
            Some: etag.Equals,
            () => false);
    }

    public static bool operator !=(EntityTag etag, string value)
    {
        return !etag.Value.Equals(value);
    }

    public static bool operator !=(EntityTag etag, StringValues value)
    {
        return !etag.Value.Equals(value);
    }

    public static bool operator !=(EntityTag etag, Option<EntityTag> value)
    {
        return value.Match(
            Some: etag.Equals,
            () => true);
    }

    /// <summary>
    /// Generates an HTTP ETag value based on the serialized representation of the current data transfer object
    /// (DTO).
    /// </summary>
    /// <remarks>This method uses SHA256 to compute the hash for the ETag. If cryptographic strength
    /// is not required, consider using a faster hashing algorithm such as xxHash to improve performance.</remarks>
    /// <param name="hashedBytes">The <see cref="byte"/> array that contains the hashed bytes</param>
    /// <param name="bytesWritten">Number of bytes written</param>
    /// <returns>A string containing the ETag, formatted as a weak validator and consisting of a base64-encoded SHA256 hash
    /// of the serialized data.</returns>
    /// <exception cref="OutOfMemoryException">When memory is full</exception>
    private static string GetStrongETag(Span<byte> hashedBytes, int bytesWritten)
    {
        Span<byte> strongHashBytes = stackalloc byte[256];
        int strongBytesWritten = SHA256.HashData(hashedBytes, strongHashBytes);
        string hex = Convert.ToHexString(strongHashBytes[..strongBytesWritten]);
        return $@"""{hex}""";
    }

    /// <summary>
    /// Generates an HTTP ETag value based on the serialized representation of the current data transfer object
    /// (DTO).
    /// </summary>
    /// <remarks>This method uses SHA256 to compute the hash for the ETag. If cryptographic strength
    /// is not required, consider using a faster hashing algorithm such as xxHash to improve performance.</remarks>
    /// <param name="data">The data to generate entity tag from.</param>
    /// <param name="bytes">A span of bytes where the data will be stored. Ensure that the destination span has sufficient length to hold the hash output (e.g., 32 bytes for SHA256).</param>
    /// <param name="hashBytes">A span of bytes where the computed hash will be stored. Ensure that the destination span has sufficient length to hold the hash output (e.g., 32 bytes for SHA256).</param>
    /// <returns>A string containing the ETag, formatted as a weak validator and consisting of a base64-encoded SHA256 hash
    /// of the serialized data.</returns>
    /// <exception cref="OutOfMemoryException">When memory is full</exception>
    private static int GetStrongETagBytes<T>(
        T data,
        Span<byte> bytes,
        Span<byte> hashBytes
    ) where T : class, ITaggableEntity
    {
        _ = data.WriteBinary(bytes);
        int hashBytesWritten = SHA256.HashData(bytes, hashBytes);
        return hashBytesWritten;
    }

    /// <summary>
    /// Generates an HTTP ETag value based on the serialized representation of the current data transfer object
    /// (DTO).
    /// </summary>
    /// <remarks>This method uses SHA256 to compute the hash for the ETag. If cryptographic strength
    /// is not required, consider using a faster hashing algorithm such as xxHash to improve performance.</remarks>
    /// <param name="hashedBytes">The <see cref="byte"/> array that contains the hashed bytes</param>
    /// <param name="bytesWritten">Number of bytes written</param>
    /// <returns>A string containing the ETag, formatted as a weak validator and consisting of a base64-encoded SHA256 hash
    /// of the serialized data.</returns>
    /// <exception cref="OutOfMemoryException">When memory is full</exception>
    private static string GetWeakETag(Span<byte> hashedBytes, int bytesWritten)
    {
        string base64 = Convert.ToBase64String(hashedBytes[..bytesWritten]);
        return $@"W/""{base64}""";
    }

    /// <summary>
    /// Generates an HTTP ETag value based on the serialized representation of the current data transfer object
    /// (DTO).
    /// </summary>
    /// <remarks>This method uses SHA256 to compute the hash for the ETag. If cryptographic strength
    /// is not required, consider using a faster hashing algorithm such as xxHash to improve performance.</remarks>
    /// <param name="data">The data to generate entity tag from.</param>
    /// <param name="ms">When this method returns, contains a MemoryStream with the serialized DTO data using Protobuf serialization.</param>
    /// <param name="bytes">A span of bytes where the data will be stored. Ensure that the destination span has sufficient length to hold the hash output (e.g., 32 bytes for SHA256).</param>
    /// <param name="hashBytes">A span of bytes where the computed hash will be stored. Ensure that the destination span has sufficient length to hold the hash output (e.g., 32 bytes for SHA256).</param>
    /// <returns>A string containing the ETag, formatted as a weak validator and consisting of a base64-encoded SHA256 hash
    /// of the serialized data.</returns>
    /// <exception cref="OutOfMemoryException">When memory is full</exception>
    private static int GetWeakETagBytes<T>(
        T data,
        MemoryStream ms,
        Span<byte> bytes,
        Span<byte> hashBytes)
    {
        JsonSerializer.Serialize(ms, data);
        bytes = ms.ToArray().AsSpan();
        // You can use SHA256 or a faster hash like xxHash if you don’t need crypto strength.
        return SHA256.HashData(bytes, hashBytes);
    }

    /// <summary>
    /// Generates an HTTP ETag value based on the serialized representation of the current data transfer object
    /// (DTO).
    /// </summary>
    /// <remarks>This method uses SHA256 to compute the hash for the ETag. If cryptographic strength
    /// is not required, consider using a faster hashing algorithm such as xxHash to improve performance.</remarks>
    /// <param name="data">The data to generate entity tag from.</param>
    /// <param name="ms">When this method returns, contains a MemoryStream with the serialized DTO data using Protobuf serialization.</param>
    /// <param name="hashBytes">A span of bytes where the computed hash will be stored. Ensure that the destination span has sufficient length to hold the hash output (e.g., 32 bytes for SHA256).</param>
    /// <returns>A string containing the ETag, formatted as a weak validator and consisting of a base64-encoded SHA256 hash
    /// of the serialized data.</returns>
    /// <exception cref="OutOfMemoryException">When memory is full</exception>
    private static async Task<int> GetWeakETagBytesAsync<T>(
        T data,
        MemoryStream ms,
        Memory<byte> hashBytes)
    {
        await JsonSerializer.SerializeAsync(ms, data);
        // You can use SHA256 or a faster hash like xxHash if you don’t need crypto strength.
        return await SHA256.HashDataAsync(ms, hashBytes);
    }

    public Result<Unit> WriteTo(HttpResponse? response)
    {
        if (response is null)
        {
            return new Result<Unit>(
                new ArgumentNullException(nameof(response)));
        }
        response.Headers.ETag = Value;
        return Unit.Default;
    }

    public override readonly string ToString()
    {
        return Value.ToString();
    }

    public bool Equals(StringValues other)
    {
        return Value.Equals(other);
    }

    public bool Equals(string? other)
    {
        return Value.Equals(other);
    }

    public bool Equals(Option<EntityTag> other)
    {
        return other.Match(
            Some: Equals,
            None: () => false);
    }

    public bool Equals(Result<EntityTag> other)
    {
        return other.Match(
            Succ: Equals,
            Fail: _ => false);
    }

    public bool Equals(OptionalResult<EntityTag> other)
    {
        return other.Match(
            Some: Equals,
            None: () => false,
            Fail: _ => false);
    }
}