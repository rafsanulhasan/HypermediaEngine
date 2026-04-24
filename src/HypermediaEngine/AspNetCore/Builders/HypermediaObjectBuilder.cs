
using HypermediaEngine.Abstractions;
using HypermediaEngine.Responses;

using EntityTagCaching.Models;

using Asp.Versioning;

using System.Globalization;

using Ardalis.GuardClauses;

namespace HypermediaEngine.Builders;

/// <summary>
/// Provides a builder for constructing hypermedia responses by configuring data, links, and metadata for a specified
/// resource type.
/// </summary>
/// <remarks>Use this class to fluently assemble a hypermedia response by chaining methods to set the resource
/// data, add hypermedia links, and attach additional metadata. The final response can be created by calling the Build
/// method. This builder is typically used to facilitate the creation of responses that follow the HATEOAS (Hypermedia
/// as the Engine of Application State) principle in RESTful APIs.</remarks>
/// <typeparam name="T">The type of the resource data to include in the hypermedia response.</typeparam>
public class HypermediaObjectBuilder<T> : IHypermediaObjectBuilder<T> 
    where T : notnull
{
    private T? _data;
    private readonly ObjectLinkCollection _links = new();
    private ObjectResponseMetadata? _metadata;

    /// <inheritdoc />
    public IHypermediaObjectBuilder<T> WithData(T data)
    {
        ArgumentNullException.ThrowIfNull(data);
        _data = data;
        return this;
    }

    /// <inheritdoc />
    public IHypermediaObjectBuilder<T> WithSelfLink(string href, string method = "GET", string? title = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(href);

        _links.Self = new HypermediaLink(href, method, LinkRelations.Self, typeof(T).Name, title);
        return this;
    }

    /// <inheritdoc />
    public IHypermediaObjectBuilder<T> WithSelfLink(HypermediaLink link)
    {
        ArgumentNullException.ThrowIfNull(link);
        ArgumentException.ThrowIfNullOrWhiteSpace(link.Relationship, nameof(link));

        _links.Self = link;
        return this;
    }

    /// <inheritdoc />
    public IHypermediaObjectBuilder<T> WithStateTransitionLink(LinkRelations rel, string href, string method = "GET", string? title = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rel);
        ArgumentException.ThrowIfNullOrWhiteSpace(href);

        _links.StateTransitions = _links.StateTransitions?.Add(rel, new HypermediaLink(href, method, rel, typeof(T).Name, title))
                               ?? new LinkCollection() { [rel] = new HypermediaLink(href, method, rel, typeof(T).Name, title) };
        return this;
    }

    /// <inheritdoc />
    public IHypermediaObjectBuilder<T> WithStateTransitionLink(HypermediaLink link)
    {
        ArgumentNullException.ThrowIfNull(link);
        ArgumentException.ThrowIfNullOrWhiteSpace(link.Relationship, nameof(link));

        _links.StateTransitions = _links.StateTransitions?.Add(link.Relationship, link)
                               ?? new LinkCollection() { [link.Relationship] = link };
        return this;
    }

    /// <inheritdoc />
    public IHypermediaObjectBuilder<T> WithRelatedLink(LinkRelations rel, string href, string method = "GET", string? title = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rel);
        ArgumentException.ThrowIfNullOrWhiteSpace(href);

        _links.Related = _links.Related?.Add(rel, new HypermediaLink(href, method, rel, typeof(T).Name, title))
                      ?? new LinkCollection() { [rel] = new HypermediaLink(href, method, rel, typeof(T).Name, title) };
        return this;
    }

    /// <inheritdoc />
    public IHypermediaObjectBuilder<T> WithRelatedLink(HypermediaLink link)
    {
        ArgumentNullException.ThrowIfNull(link);
        ArgumentException.ThrowIfNullOrWhiteSpace(link.Relationship, nameof(link));

        _links.Related = _links.Related?.Add(link.Relationship, link)
                      ?? new LinkCollection() { [link.Relationship] = link };
        return this;
    }

    /// <inheritdoc />
    public IHypermediaObjectBuilder<T> WithApiVersion(ApiVersion version)
    {
        ArgumentNullException.ThrowIfNull(version);
        _metadata = _metadata is null
            ? new ObjectResponseMetadata(EntityTag.Empty, version.ToString("{0}", CultureInfo.InvariantCulture))
            : _metadata with
            {
                ApiVersion = version.ToString("{0}", CultureInfo.InvariantCulture),
            };
        return this;
    }

    public IHypermediaObjectBuilder<T> WithMetadata(ObjectResponseMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        _metadata = metadata;
        return this;
    }

    public IHypermediaObjectBuilder<T> WithDomainMetadata(string key, string value)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentException.ThrowIfNullOrEmpty(value);
        _metadata = _metadata is null
                  ? new ObjectResponseMetadata(
                      EntityTag: EntityTag.Empty,
                      ApiVersion: null,
                      EntityVersion: null,
                      Domain: new DomainMetadata { [key] = value })
                  : _metadata with
                  {
                      Domain = _metadata.Domain is null
                             ? new DomainMetadata { [key] = value }
                             : _metadata.Domain.Add(key, value),
                  };
        return this;
    }

    /// <inheritdoc />
    public HypermediaObjectResponse<T> Build()
    {
        string message = "Data must be provided to build a hypermedia response.";
        Guard.Against.Null(_data, message, exceptionCreator: () => new InvalidOperationException(message));
        return new HypermediaObjectResponse<T>(_data)
        {
            Links = _links,
            Meta = _metadata,
        };
    }
}
