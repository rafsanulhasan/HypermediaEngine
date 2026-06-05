using System.Diagnostics.CodeAnalysis;

namespace SynergyFx.HypermediaEngine.Requests.Filtering;

internal sealed class StringFilterConditionHandler(IFilterConditionHandler nextHandler)
    : AbstractFilterConditionHandler(nextHandler)
{
    protected override string HandleCondition(FilterCondition condition, object? value)
    {
        bool isOperatorValid = condition.Operator.IsValidForType(typeof(string));
        string? valueAsString = value?.ToString();
        string result = (condition.Operator.Name, isOperatorValid) switch
        {
            (FilterOperator.EqKey, true) when valueAsString is null => $"x.{condition.Field} == null",
            (FilterOperator.EqKey, true) => $"x.{condition.Field} == \"{valueAsString}\"",
            (FilterOperator.NeKey, true) when valueAsString is null => $"x.{condition.Field} != null",
            (FilterOperator.NeKey, true) => $"x.{condition.Field} != \"{valueAsString}\"",
            (FilterOperator.ContainsKey, true) => $"x.{condition.Field}.Contains(\"{valueAsString}\")",
            (FilterOperator.NotContainsKey, true) => $"!x.{condition.Field}.Contains(\"{valueAsString}\")",
            (FilterOperator.StartsWithKey, true) => $"x.{condition.Field}.StartsWith(\"{valueAsString}\")",
            (FilterOperator.NotStartsWithKey, true) => $"!x.{condition.Field}.StartsWith(\"{valueAsString}\")",
            (FilterOperator.EndsWithKey, true) => $"x.{condition.Field}.EndsWith(\"{valueAsString}\")",
            (FilterOperator.NotEndsWithKey, true) => $"!x.{condition.Field}.EndsWith(\"{valueAsString}\")",
            _ => throw new NotSupportedException($"The filter operator '{condition.Operator}' is not supported for type '{condition.Value?.GetType().Name}'."),
        };
        return result;
    }

    protected override bool ShouldHandle(
        FilterCondition condition,
        [NotNullWhen(true)] out object? value)
    {
        if (condition.Value is null)
        {
            value = null;
#pragma warning disable CS8762 // Parameter must have a non-null value when exiting in some condition.
            return true;
#pragma warning restore CS8762 // Parameter must have a non-null value when exiting in some condition.
        }
        if (condition.Value is string s)
        {
            value = s;
            return true;
        }

        value = null;
        return false;
    }
}
