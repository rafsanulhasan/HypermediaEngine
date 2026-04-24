using HypermediaEngine.Responses;

using System.Text.Json.Serialization;

public abstract record class AbstractResponse<T>
    where T : notnull
{
    protected AbstractResponse(T data)
    {
        Data = data;
    }

    protected AbstractResponse(
        T data,
        ResponseMetadata meta,
        ObjectLinkCollection? links = null
    ) : this(data)
    {
        Meta = meta;
        Links = links;
    }

    public T Data { get; internal init; }

    [JsonPropertyName("_meta")]
    public ResponseMetadata? Meta { get; internal init; }
    [JsonPropertyName("_links")]
    public ObjectLinkCollection? Links { get; internal init; }
}
