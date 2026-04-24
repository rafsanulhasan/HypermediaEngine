using HypermediaEngine.Responses;

namespace HypermediaEngine.Abstractions;

public interface ILinkCollection : IDictionary<string, HypermediaLink>;

public interface IListLinkCollection
{
    HypermediaLink Self { get; }
    LinkCollection Paging { get; }
}

