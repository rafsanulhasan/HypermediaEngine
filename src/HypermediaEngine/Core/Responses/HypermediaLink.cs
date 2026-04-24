using HypermediaEngine.Abstractions;

using System.Text.Json.Serialization;

namespace HypermediaEngine.Responses;

/// <summary>
/// Represents a hypermedia link to a resource, including its URL, HTTP method, relationship type, and additional
/// metadata.
/// </summary>
/// <remarks>This record class is used to encapsulate the details of a hypermedia link, which can be utilized in
/// hypermedia-driven APIs. The properties allow for flexible representation of the link's characteristics, such as its
/// method and relationship to other resources.</remarks>
public sealed record class HypermediaLink
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HypermediaLink"/> class with the specified parameters.
    /// </summary>
    /// <param name="href">The URL of the linked resource.</param>
    /// <param name="method">The HTTP method to be used for the request.</param>
    /// <param name="rel">The relationship type of the linked resource.</param>
    /// <param name="type">The type of the resource being linked to.</param>
    /// <param name="title">The title of the linked resource.</param>
    public HypermediaLink(string href, string method, LinkRelations? rel = null, string? type = null, string? title = null)
    {
        Href = href;
        Method = method;
        Relationship = rel;
        Type = type;
        Title = title;
    }

    /// <summary>
    /// Gets or initializes the URL of the linked resource.
    /// </summary>
    public string Href { get; init; } = string.Empty;

    /// <summary>
    /// Gets or initializes the HTTP method to be used for the request.
    /// </summary>
    /// <remarks>The default value is "GET". This property can be set to other standard HTTP methods such as
    /// "POST", "PUT", or "DELETE" as required by the request.</remarks>
    public string Method { get; init; } = "GET";

    /// <summary>
    /// Gets or initializes the title of the item.
    /// </summary>
    /// <remarks>The title provides a brief description or name for the item. If not set, the value is
    /// null.</remarks>
    public string? Title { get; init; } = null;

    /// <summary>
    /// Gets or initializes the type associated with the current instance as a string identifier.
    /// </summary>
    /// <remarks>This property may be null if no type has been specified. The value is intended to represent
    /// the logical or semantic type of the instance for identification or processing purposes.</remarks>
    public string? Type { get; init; } = null;

    /// <summary>
    /// Gets or initializes the relationship type of the resource, which defines the nature of the link to another resource.
    /// </summary>
    /// <remarks>The relationship type is commonly used in hypermedia APIs to indicate how the current
    /// resource relates to another. This property can be set to null if no specific relationship is defined.</remarks>
    [JsonPropertyName("rel")]
    public LinkRelations? Relationship { get; init; } = null;
}

[JsonSerializable(typeof(IObjectLinkCollection))]
[JsonSerializable(typeof(ObjectLinkCollection))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    RespectNullableAnnotations = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.Always)]
internal sealed partial class ObjectLinkCollectionJsonSerializerContext
    : JsonSerializerContext
{
}


