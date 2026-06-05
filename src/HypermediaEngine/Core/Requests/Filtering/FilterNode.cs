using System.Text.Json.Serialization;

namespace SynergyFx.HypermediaEngine.Requests.Filtering;

public sealed record FilterNode
{
    public FilterNode(IReadOnlyList<FilterCondition> conditions)
    {
        Conditions = conditions;
    }

    public FilterNode(
        FilterLogic? logic,
        IReadOnlyList<FilterCondition>? conditions,
        IReadOnlyList<FilterNode>? children = null
    ) : this(conditions ?? [])
    {
        Logic = logic;
        Children = children;
    }

    [JsonConstructor]
    internal FilterNode() { }

    public FilterLogic? Logic { get; set; }
    public IReadOnlyList<FilterCondition>? Conditions { get; set; }
    public IReadOnlyList<FilterNode>? Children { get; set; }
}
