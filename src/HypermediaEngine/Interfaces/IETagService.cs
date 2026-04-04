namespace HypermediaEngine.Interfaces;

public interface IETagService
{
    string GenerateETag(object resource);
    bool IsETagStale(string etag, string? ifNoneMatch);
}
