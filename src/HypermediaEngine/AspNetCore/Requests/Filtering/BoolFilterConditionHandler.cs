using System.Diagnostics.CodeAnalysis;

namespace SynergyFx.HypermediaEngine.Requests.Filtering;

internal sealed class BoolFilterConditionHandler(IFilterConditionHandler nextHandler)
    : AbstractFilterConditionHandler(nextHandler)
{
    protected override string HandleCondition(FilterCondition condition, object? value)
    {
        bool isOperatorValid = condition.Operator.IsValidForType(typeof(bool));
        return (condition.Operator.Name, isOperatorValid) switch
        {
            (FilterOperator.EqKey, true) => $"x.{condition.Field} == {value!.ToString()?.ToLower()}",
            (FilterOperator.NeKey, true) => $"x.{condition.Field} != {value!.ToString()?.ToLower()}",
            _ => throw new NotSupportedException($"The filter operator '{condition.Operator}' is not supported for type '{condition.Value?.GetType().Name}'."),
        };
    }

    protected override bool ShouldHandle(
        FilterCondition condition,
        [NotNullWhen(true)] out object? value)
    {
        if (condition.Value is bool b)
        {
            value = b;
            return true;
        }

        value = null;
        return false;
    }
}
