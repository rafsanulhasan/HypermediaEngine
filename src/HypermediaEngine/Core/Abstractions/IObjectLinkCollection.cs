using HypermediaEngine.Responses;

namespace HypermediaEngine.Abstractions;

public interface IObjectLinkCollection
{
    HypermediaLink Self { get; }
    LinkCollection? StateTransitions { get; }
    LinkCollection? Related { get; }
}

