using EntityTagCaching.Models;

using HypermediaEngine.Abstractions;
using HypermediaEngine.Requests.Paging;
using HypermediaEngine.Responses;

namespace HypermediaEngine.Builders;

/// <summary>
/// Provides a builder for constructing hypermedia collection responses that include a list of items, associated links,
/// and optional metadata for use in hypermedia APIs.
/// </summary>
/// <typeparam name="T">The type of the items contained in the hypermedia collection.</typeparam>
public sealed class HypermediaCollectionBuilder<T> : IHypermediaCollectionBuilder<T>
    where T : notnull
{
    public IEnumerable<T> Items { get; private set; }
    public IEnumerable<HypermediaObjectResponse<T>> HalItems { get; private set; }
    public ListLinkCollection Links { get; private set; } = new();
    public ListResponseMetadata? Metadata { get; private set; }

    /// <inheritdoc />
    public IHypermediaCollectionBuilder<T> WithItems(IEnumerable<T> items)
    {
        Items = items;
        return this;
    }

    /// <inheritdoc />
    public IHypermediaCollectionBuilder<T> WithItems(IEnumerable<HypermediaObjectResponse<T>> items)
    {
        HalItems = items;
        return this;
    }

    /// <inheritdoc />
    public IHypermediaCollectionBuilder<T> WithSelfLink(string href, string method = "GET", string? title = null)
    {
        HypermediaLink link = new(href, method, LinkRelations.Self, $"ListOf{typeof(T).Name}", title);
        return WithSelfLink(link);
    }

    /// <inheritdoc />
    public IHypermediaCollectionBuilder<T> WithSelfLink(HypermediaLink link)
    {
        ArgumentNullException.ThrowIfNull(link);
        ArgumentException.ThrowIfNullOrWhiteSpace(link.Relationship, nameof(link));

        Links.Self = link;
        return this;
    }

    public IHypermediaCollectionBuilder<T> WithStateTransitionLink(
        LinkRelations rel,
        string href,
        string method = "GET",
        string? type = null,
        string? title = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(href);
        ArgumentException.ThrowIfNullOrEmpty(method);

        HypermediaLink link = new(href, method, rel, type, title);
        return WithStateTransitionLink(link);
    }

    public IHypermediaCollectionBuilder<T> WithStateTransitionLink(HypermediaLink link)
    {
        ArgumentNullException.ThrowIfNull(link);

        Links.StateTransitions ??= [];
        if (link is { Relationship: not null })
        {
            Links.StateTransitions[link.Relationship] = link;
        }
        return this;
    }

    public IHypermediaCollectionBuilder<T> WithRelatedLink(
        LinkRelations rel,
        string href,
        string method = "GET",
        string? type = null,
        string? title = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(href);
        ArgumentException.ThrowIfNullOrEmpty(method);

        HypermediaLink link = new(href, method, rel, type, title);
        return WithRelatedLink(link);
    }

    public IHypermediaCollectionBuilder<T> WithRelatedLink(HypermediaLink link)
    {
        ArgumentNullException.ThrowIfNull(link);

        Links.Related ??= [];
        if (link is { Relationship: not null })
        {
            Links.Related[link.Relationship] = link;
        }
        return this;
    }

    public IHypermediaCollectionBuilder<T> WithPageLink(
        string href,
        LinkRelations rel,
        string method = "GET",
        string? type = null,
        string? title = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(href);
        ArgumentException.ThrowIfNullOrEmpty(method);

        HypermediaLink link = new(href, method, rel, type, title);
        return WithPageLink(link);
    }

    public IHypermediaCollectionBuilder<T> WithPageLink(HypermediaLink link)
    {
        ArgumentNullException.ThrowIfNull(link);

        Links.Paging ??= [];
        if (!string.IsNullOrWhiteSpace(link.Title))
        {
            Links.Paging[link.Title!] = link;
        }
        return this;
    }

    /// <inheritdoc />
    public HypermediaCollectionResponse<T> Build()
    {
        return new HypermediaCollectionResponse<T>(Items, Items.Count())
        {
            Links = Links,
            Meta = Metadata,
        };
    }

    /// <inheritdoc />
    public IHypermediaCollectionBuilder<T> WithMetadata(ListResponseMetadata metadata)
    {
        Metadata ??= new ListResponseMetadata(EntityTag.Empty);
        Metadata = Metadata with
        {
            ApiVersion = metadata.ApiVersion,
            Domain = metadata.Domain,
            EntityTag = metadata.EntityTag,
            Filters = metadata.Filters,
            Paging = metadata.Paging,
            Sorting = metadata.Sorting,
        };
        return this;
    }
}
