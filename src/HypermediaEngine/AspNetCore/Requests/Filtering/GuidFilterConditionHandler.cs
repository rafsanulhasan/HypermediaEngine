using System.Diagnostics.CodeAnalysis;

namespace SynergyFx.HypermediaEngine.Requests.Filtering;

internal sealed class GuidFilterConditionHandler(IFilterConditionHandler nextHandler)
    : AbstractFilterConditionHandler(nextHandler)
{
    protected override string HandleCondition(FilterCondition condition, object? value)
    {
        bool isOperatorValid = condition.Operator.IsValidForType(typeof(Guid));
        return (condition.Operator.Name, isOperatorValid) switch
        {
            (FilterOperator.EqKey, true) => $"x.{condition.Field} == \"{value}\"",
            (FilterOperator.NeKey, true) => $"x.{condition.Field} != \"{value}\"",
            _ => throw new NotSupportedException($"The filter condition '{condition.Operator}' is not supported for type '{condition.Value?.GetType().Name}'."),

        };
    }

    protected override bool ShouldHandle(
        FilterCondition condition,
        [NotNullWhen(true)] out object? value)
    {
        if (condition.Value is Guid g)
        {
            value = g;
            return true;
        }

        value = null;
        return false;
    }
}
