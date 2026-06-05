using System.Diagnostics.CodeAnalysis;

namespace SynergyFx.HypermediaEngine.Requests.Filtering;

internal sealed class DateTimeFilterConditionHandler(IFilterConditionHandler nextHandler)
    : AbstractFilterConditionHandler(nextHandler)
{
    protected override string HandleCondition(FilterCondition condition, object? value)
    {
        bool isOperatorValid = condition.Operator.IsValidForType(typeof(DateTimeOffset));
        return (condition.Operator.Name, isOperatorValid) switch
        {
            (FilterOperator.EqKey, true) => $"x.{condition.Field} == \"{value}\"",
            (FilterOperator.NeKey, true) => $"x.{condition.Field} != \"{value}\"",
            (FilterOperator.GteKey, true) => $"x.{condition.Field} >= \"{value}\"",
            (FilterOperator.LteKey, true) => $"x.{condition.Field} <= \"{value}\"",
            (FilterOperator.GtKey, true) => $"x.{condition.Field} > \"{value}\"",
            (FilterOperator.LtKey, true) => $"x.{condition.Field} < \"{value}\"",
            _ => throw new NotSupportedException($"The filter condition '{condition.Operator}' is not supported for type '{condition.Value?.GetType().Name}'."),
        };
    }

    protected override bool ShouldHandle(
        FilterCondition condition,
        [NotNullWhen(true)] out object? value)
    {

        if (condition.Value is DateTimeOffset dto)
        {
            value = dto;
            return true;
        }

        if (condition.Operator.IsValidForType(typeof(DateTime))
         && condition.Value is DateTime dt)
        {
            value = dt;
            return true;
        }

        value = null;
        return false;
    }
}
