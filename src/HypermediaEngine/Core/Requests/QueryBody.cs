using HypermediaEngine.Requests.Filtering;
using HypermediaEngine.Requests.Sorting;

using System.Text.Json.Serialization;

namespace HypermediaEngine.Requests;

public sealed record class QueryBody
{
    [JsonConstructor]
    public QueryBody() { }

    public FilterNode? Filtering { get; set; }
    public IReadOnlyList<SortField>? Sorting { get; set; }
}
