namespace HypermediaEngine.Services;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using HypermediaEngine.Interfaces;

public class ETagService : IETagService
{
    private readonly JsonSerializerOptions _jsonOptions;

    public ETagService()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public string GenerateETag(object resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        var json = JsonSerializer.Serialize(resource, _jsonOptions);
        var bytes = Encoding.UTF8.GetBytes(json);
        var hash = SHA256.HashData(bytes);
        return $"\"{Convert.ToHexString(hash)[..16]}\"";
    }

    public bool IsETagStale(string etag, string? ifNoneMatch)
    {
        if (string.IsNullOrEmpty(ifNoneMatch))
            return true;

        return !string.Equals(etag, ifNoneMatch, StringComparison.OrdinalIgnoreCase);
    }
}
