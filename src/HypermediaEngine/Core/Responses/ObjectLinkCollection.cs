using HypermediaEngine.Abstractions;

namespace HypermediaEngine.Responses;

public sealed partial record class ObjectLinkCollection
    : IObjectLinkCollection
{
#pragma warning disable IDE0044 // Add readonly modifier
    private LinkCollection? _stateTranstiotions;
    private LinkCollection? _related;
#pragma warning restore IDE0044 // Add readonly modifier

    private ObjectLinkCollection(HypermediaLink self)
    {
        Self = self;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    internal ObjectLinkCollection() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public HypermediaLink Self { get; set; }

    public LinkCollection? StateTransitions
    {
        get => _stateTranstiotions;
        internal set => _stateTranstiotions = value;
    }

    public LinkCollection? Related
    {
        get => _related;
        internal set => _related = value;
    }

    public static ObjectLinkCollection Create(
        HypermediaLink self,
        IDictionary<LinkRelations, HypermediaLink>? stateTransitions = null,
        IDictionary<LinkRelations, HypermediaLink>? related = null)
    {
        LinkCollection? stateTransitionsLinkCollection = stateTransitions is null or { Count: 0 }
                             ? null
                             : LinkCollection.Create(stateTransitions);
        ObjectLinkCollection collection = new(self)
        {
            _stateTranstiotions = stateTransitionsLinkCollection,
            _related = related is null or { Count: 0 }
                     ? null
                     : LinkCollection.Create(related),
        };
        return collection;
    }
}


