using HypermediaEngine.Abstractions;

using System.Text.Json.Serialization;

namespace HypermediaEngine.Responses;

public sealed class ListLinkCollection
    : IListLinkCollection
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    internal ListLinkCollection()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
    }

    public HypermediaLink Self { get; internal set; }
    public LinkCollection StateTransitions { get; internal set; }
    public LinkCollection Related { get; internal set; }

    public LinkCollection Paging { get; internal set; }

    [JsonIgnore]
    public ListResponseMetadata Meta { get; init; }

    internal static ListLinkCollection Create(
        HypermediaLink self,
        ListResponseMetadata metadata,
        IDictionary<LinkRelations, HypermediaLink> paging)
    {
        ListLinkCollection linkCollection = new()
        {
            Self = self,
            Meta = metadata,
            Paging = LinkCollection.Create(paging),
        };
        return linkCollection;
    }
}


