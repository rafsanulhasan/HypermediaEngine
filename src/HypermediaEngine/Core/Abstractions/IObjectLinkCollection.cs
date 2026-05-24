using HypermediaEngine.Responses;

using SynergyFx.HypermediaEngine.Responses;

namespace SynergyFx.HypermediaEngine.Abstractions;

public interface IObjectLinkCollection
{
    HypermediaLink Self { get; }
    LinkCollection? StateTransitions { get; }
    LinkCollection? Related { get; }
}

