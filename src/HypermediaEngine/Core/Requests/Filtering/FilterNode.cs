using LanguageExt;

namespace HypermediaEngine.Requests.Filtering;

public sealed record FilterNode
{
    public FilterNode(
        FilterLogic logic,
        IReadOnlyList<FilterCondition>? conditions,
        IReadOnlyList<FilterNode>? children)
    {
        Logic = logic;
        Conditions = conditions;
        Children = children;
    }

    internal FilterNode() { }

    public FilterLogic Logic { get; set; }
    public IReadOnlyList<FilterCondition>? Conditions { get; set; }
    public IReadOnlyList<FilterNode>? Children { get; set; }

    public override string ToString()
    {
        List<string> parts = [];

        if (Conditions is not null)
        {
            foreach (FilterCondition condition in Conditions)
            {
                string conditionString = FormatCondition(condition);
                parts.Add(conditionString);
            }
        }

        if (Children is not null)
        {
            foreach (FilterNode child in Children)
            {
                string childString = child.ToString();
                if (!string.IsNullOrWhiteSpace(childString))
                {
                    bool needsParentheses = child.Logic != Logic && 
                                           ((child.Conditions?.Count ?? 0) + (child.Children?.Count ?? 0)) > 1;
                    parts.Add(needsParentheses ? $"({childString})" : childString);
                }
            }
        }

        if (parts.Count == 0)
            return string.Empty;

        string logicOperator = Logic == FilterLogic.And ? " && " : " || ";
        string result = string.Join(logicOperator, parts);

        return result;
    }

    private static string FormatCondition(FilterCondition condition)
    {
        return (condition.Value, condition.Operator.Name) switch
        {
            (IEnumerable<string> list, FilterOperator.InKey) => 
                $"{condition.Field} in ({string.Join(", ", list.Select(i => $"'{i}'"))})",
            (IEnumerable<string> list, FilterOperator.NotInKey) => 
                $"{condition.Field} not in ({string.Join(", ", list.Select(i => $"'{i}'"))})",
            (IEnumerable<int> list, FilterOperator.InKey) => 
                $"{condition.Field} in ({string.Join(", ", list)})",
            (IEnumerable<int> list, FilterOperator.NotInKey) => 
                $"{condition.Field} not in ({string.Join(", ", list)})",
            (string value, FilterOperator.EqKey) => 
                $"{condition.Field} == \"{value}\"",
            (string value, FilterOperator.NeKey) => 
                $"{condition.Field} != \"{value}\"",
            (string value, FilterOperator.ContainsKey) => 
                $"{condition.Field} like \"%{value}%\"",
            (string value, FilterOperator.StartsWithKey) => 
                $"{condition.Field} like \"{value}%\"",
            (string value, FilterOperator.EndsWithKey) => 
                $"{condition.Field} like \"%{value}\"",
            (bool value, FilterOperator.EqKey) => 
                $"{condition.Field} == {value}",
            (bool value, FilterOperator.NeKey) => 
                $"{condition.Field} != {value}",
            (int or uint or long or ulong or short or ushort or float or double or decimal, FilterOperator.EqKey) => 
                $"{condition.Field} == {condition.Value}",
            (int or uint or long or ulong or short or ushort or float or double or decimal, FilterOperator.NeKey) => 
                $"{condition.Field} != {condition.Value}",
            (int or uint or long or ulong or short or ushort or float or double or decimal, FilterOperator.GtKey) => 
                $"{condition.Field} > {condition.Value}",
            (int or uint or long or ulong or short or ushort or float or double or decimal, FilterOperator.GteKey) => 
                $"{condition.Field} >= {condition.Value}",
            (int or uint or long or ulong or short or ushort or float or double or decimal, FilterOperator.LtKey) => 
                $"{condition.Field} < {condition.Value}",
            (int or uint or long or ulong or short or ushort or float or double or decimal, FilterOperator.LteKey) => 
                $"{condition.Field} <= {condition.Value}",
            _ => throw new NotSupportedException($"Unsupported combination: {condition.Value?.GetType().Name} with {condition.Operator}")
        };
    }
}
