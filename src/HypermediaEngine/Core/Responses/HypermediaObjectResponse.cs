namespace HypermediaEngine.Responses;

/// <summary>
/// Represents a hypermedia-enriched response wrapping a single resource of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of the resource payload.</typeparam>
public sealed record class HypermediaObjectResponse<T>(T data) : AbstractResponse<T>(data)
    where T : notnull
{
}
