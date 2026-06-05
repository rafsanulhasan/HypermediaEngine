using SynergyFx.HypermediaEngine.Requests.Filtering;
using SynergyFx.HypermediaEngine.Requests.Sorting;

using System.Text.Json.Serialization;

namespace SynergyFx.HypermediaEngine.Requests;

public sealed record class QueryBody
{
    [JsonConstructor]
    public QueryBody() { }

    public FilterNode? Filtering { get; set; }
    public IReadOnlyList<SortField>? Sorting { get; set; }
}
