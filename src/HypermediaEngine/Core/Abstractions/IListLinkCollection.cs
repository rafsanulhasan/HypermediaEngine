using HypermediaEngine.Responses;

using SynergyFx.HypermediaEngine.Responses;

namespace SynergyFx.HypermediaEngine.Abstractions;

public interface ILinkCollection : IDictionary<string, HypermediaLink>;

public interface IListLinkCollection
{
    HypermediaLink Self { get; }
    LinkCollection Paging { get; }
}

