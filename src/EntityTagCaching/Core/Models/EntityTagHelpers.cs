using EntityTagCaching.Abstractions;

using LanguageExt;
using LanguageExt.Common;

using System.Security.Cryptography;
using System.Text.Json;

namespace EntityTagCaching.Models;

#pragma warning disable CA1031 // Do not catch general exception types
public static class EntityTagHelpers
{
    extension(object? dto)
    {
        public OptionalResult<string> GetETag()
        {
            if (dto is null)
            {
                return OptionalResult<string>.Optional(string.Empty);
            }
            MemoryStream ms = new();
            try
            {
                Span<byte> hashBytes = stackalloc byte[256];
                int bytesWritten;
                if (dto is ITaggableEntity entityTaggable)
                {
                    bytesWritten = entityTaggable.WriteBinary(hashBytes);
                    string strongEtag = GetStrongETag(hashBytes, bytesWritten);
                    return strongEtag;
                }

                bytesWritten = dto.GetWeakETagBytes(ms, hashBytes);
                string weakEtag = GetWeakETag(hashBytes, bytesWritten);
                return weakEtag;
            }
            catch (Exception ex)
            {
                return new OptionalResult<string>(ex);
            }
            finally
            {
                ms.Dispose();
            }
        }

        /// <summary>
        /// Asynchronously retrieves the ETag value for the current resource.
        /// </summary>
        /// <remarks>This method uses a MemoryStream to facilitate ETag retrieval. Ensure that the
        /// underlying resource is accessible to avoid exceptions during the operation.</remarks>
        /// <returns>
        /// A Result containing the ETag as a string if retrieval succeeds;
        /// otherwise, contains exception details if an error occurs.
        /// <para>
        /// Returns <see cref="OutOfMemoryException"/> when memory is full. Handle this exception appropriately in the calling code to ensure application stability.
        /// </para>
        /// <para>
        /// Returns <see cref="ObjectDisposedException"/> if the MemoryStream is disposed before the ETag retrieval is complete. Ensure that the MemoryStream is properly managed and not disposed prematurely to avoid this exception.
        /// </para>
        /// </returns>
        public async Task<OptionalResult<string>> GetETagAsync(CancellationToken ct)
        {
            if (dto is null)
            {
                return Option<string>.None;
            }
            MemoryStream ms = new();
            try
            {
                Memory<byte> hashBytes = new(new byte[256]);
                int bytesWritten;
                if (dto is ITaggableEntity entityTaggable)
                {
                    bytesWritten = entityTaggable.WriteBinary(hashBytes.Span);
                    string strongEtag = GetStrongETag(hashBytes.Span, bytesWritten);
                    return strongEtag;
                }

                bytesWritten = await dto.GetWeakETagBytesAsync(ms, hashBytes, ct);
                string weakEtag = GetWeakETag(hashBytes, bytesWritten);
                return weakEtag;
            }
            catch (Exception ex)
            {
                return new OptionalResult<string>(ex);
            }
            finally
            {
                await ms.DisposeAsync().ConfigureAwait(false);
            }
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
            string base64 = Convert.ToHexString(strongHashBytes[..strongBytesWritten]);
            return $@"""{base64}""";
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
        private static string GetWeakETag(Memory<byte> hashedBytes, int bytesWritten)
        {
            string base64 = Convert.ToBase64String(hashedBytes.Span[..bytesWritten]);
            return $@"W/""{base64}""";
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
        /// <param name="ms">When this method returns, contains a MemoryStream with the serialized DTO data using Protobuf serialization.</param>
        /// <param name="destination">A span of bytes where the computed hash will be stored. Ensure that the destination span has sufficient length to hold the hash output (e.g., 32 bytes for SHA256).</param>
        /// <returns>A string containing the ETag, formatted as a weak validator and consisting of a base64-encoded SHA256 hash
        /// of the serialized data.</returns>
        /// <exception cref="OutOfMemoryException">When memory is full</exception>
        private int GetWeakETagBytes(MemoryStream ms, Span<byte> destination)
        {
            JsonSerializer.Serialize(ms, dto);
            ReadOnlySpan<byte> bytes = ms.ToArray().AsSpan();
            // You can use SHA256 or a faster hash like xxHash if you don’t need crypto strength.
            return SHA256.HashData(bytes, destination);
        }

        /// <summary>
        /// Generates an HTTP ETag value based on the serialized representation of the current data transfer object
        /// (DTO).
        /// </summary>
        /// <remarks>This method uses SHA256 to compute the hash for the ETag. If cryptographic strength
        /// is not required, consider using a faster hashing algorithm such as xxHash to improve performance.</remarks>
        /// <param name="ms">When this method returns, contains a MemoryStream with the serialized DTO data using Protobuf serialization.</param>
        /// <param name="destination">A span of bytes where the computed hash will be stored. Ensure that the destination span has sufficient length to hold the hash output (e.g., 32 bytes for SHA256).</param>
        /// <returns>A string containing the ETag, formatted as a weak validator and consisting of a base64-encoded SHA256 hash
        /// of the serialized data.</returns>
        /// <exception cref="OutOfMemoryException">When memory is full</exception>
        private async Task<int> GetWeakETagBytesAsync(MemoryStream ms, Memory<byte> destination, CancellationToken ct)
        {
            JsonSerializer.Serialize(ms, dto);
            ReadOnlySpan<byte> bytes = ms.ToArray().AsSpan();
            // You can use SHA256 or a faster hash like xxHash if you don’t need crypto strength.
            return await SHA256.HashDataAsync(ms, destination, ct);
        }
    }
}
