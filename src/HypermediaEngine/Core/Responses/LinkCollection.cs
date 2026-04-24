using HypermediaEngine.Abstractions;

namespace HypermediaEngine.Responses;

public sealed class LinkCollection : Dictionary<string, HypermediaLink>, ILinkCollection
{
    internal LinkCollection() : base()
    {
    }

    /// <inheritdoc />
    public new HypermediaLink this[LinkRelations key]
    {
        get => base[key];
        set => base[key] = value;
    }

    /// <inheritdoc />
    public new int Count => base.Count;

    int ICollection<KeyValuePair<string, HypermediaLink>>.Count => base.Count;

    public static LinkCollection Create()
    {
        return [];
    }

    internal static LinkCollection Create(
        IDictionary<LinkRelations, HypermediaLink> collection)
    {
        LinkCollection returnValue = Create();
        if (collection is null or { Count: 0 })
        {
            return returnValue;
        }

        foreach (KeyValuePair<LinkRelations, HypermediaLink> item in collection)
        {
            returnValue.Add(item.Key, item.Value);
        }
        return returnValue;
    }

    public LinkCollection Add(LinkRelations key, HypermediaLink value)
    {
        base.Add(key, value);
        return this;
    }

    /// <inheritdoc />
    public new bool ContainsKey(LinkRelations key)
    {
        return base.ContainsKey(key);
    }

    /// <inheritdoc />
    public new void Remove(LinkRelations key)
    {
        base.Remove(key);
    }

    /// <inheritdoc />
    public new bool TryAdd(LinkRelations key, HypermediaLink value)
    {
        return base.TryAdd(key, value);
    }

    /// <inheritdoc />
    public new bool TryGetValue(LinkRelations key, out HypermediaLink value)
    {
        return base.TryGetValue(key, out value!);
    }
}


