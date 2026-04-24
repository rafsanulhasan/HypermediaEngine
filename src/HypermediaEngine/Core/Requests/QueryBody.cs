using HypermediaEngine.Requests.Filtering;
using HypermediaEngine.Requests.Sorting;

namespace HypermediaEngine.Requests;

public sealed record class QueryBody
{
    public FilterNode? Filtering { get; set; }
    public IReadOnlyList<SortField>? Sorting { get; set; }
}
