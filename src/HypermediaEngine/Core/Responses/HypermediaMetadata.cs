using EntityTagCaching.Models;

using HypermediaEngine.Requests.Filtering;
using HypermediaEngine.Requests.Sorting;

namespace HypermediaEngine.Responses;

#pragma warning disable MA0048 // File name must match type name
public abstract partial record class ResponseMetadata(
    EntityTag EntityTag,
    string? ApiVersion = null,
    DomainMetadata? Domain = null);

public sealed partial record class ObjectResponseMetadata(
    EntityTag EntityTag,
    string? ApiVersion = null,
    int? EntityVersion = null,
    DomainMetadata? Domain = null
): ResponseMetadata(EntityTag, ApiVersion, Domain);

public sealed class DomainMetadata : Dictionary<string, object>
{
    public DomainMetadata Add(string key, object value)
    {
        base.Add(key, value);
        return this;
    }
}

public sealed partial record class ListResponseMetadata(
    EntityTag EntityTag,
    string? ApiVersion = null,
    DomainMetadata? Domain = null,
    FilterNode? Filters = null,
    PagingMetadata? Paging = null,
    IReadOnlyList<SortingMetadata>? Sorting = null
) : ResponseMetadata(EntityTag, ApiVersion, Domain);
#pragma warning restore MA0048 // File name must match type name