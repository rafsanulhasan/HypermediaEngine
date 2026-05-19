using System.Text.Json.Serialization;

namespace HypermediaEngine.Requests.Filtering;

public sealed record FilterNode
{
    public FilterNode(IReadOnlyList<FilterCondition> conditions)
    {
        Conditions = conditions;
    }

    public FilterNode(
        FilterLogic logic,
        IReadOnlyList<FilterCondition>? conditions,
        IReadOnlyList<FilterNode>? children
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

    /// <inheritdoc />
    public override string ToString()
    {
        int localConditionsCount = Conditions?.Count ?? 0;
        int childrenCount = Children?.Count ?? 0;
        int totalElements = localConditionsCount + childrenCount;

        if (Logic is null && totalElements > 1)
        {
            throw new InvalidOperationException("Logic operator (And/Or) must be specified when combining multiple conditions or child nodes.");
        }
        if (Logic is not null && totalElements == 0)
        {
            throw new InvalidOperationException("Conditions or Children is required when a Logic operator is provided.");
        }

        List<string> parts = [];
        if (Conditions is not null)
        {
            foreach (FilterCondition condition in Conditions)
            {
                string conditionString = condition.ToString();
                parts.Add(conditionString);
            }
        }

        if (Children is { Count: > 0 })
        {
            foreach (FilterNode child in Children)
            {
                string childString = child.ToString();
                if (string.IsNullOrWhiteSpace(childString))
                {
                    continue;
                }
                int childElements = (child.Conditions?.Count ?? 0) + (child.Children?.Count ?? 0);
                bool needsParentheses = child.Logic != Logic
                                     && childElements > 1;
                parts.Add(needsParentheses ? $"({childString})" : childString);
            }
        }

        if (parts.Count == 0)
        {
            return string.Empty;
        }

        if (parts.Count == 1)
        {
            return parts[0];
        }

        string logicOperator = (Logic ?? FilterLogic.And) == FilterLogic.And
                             ? " && "
                             : " || ";

        string result = string.Join(logicOperator, parts);

        return result;
    }
}
