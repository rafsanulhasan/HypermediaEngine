using System.Diagnostics.CodeAnalysis;

namespace SynergyFx.HypermediaEngine.Requests.Filtering;

internal sealed class NumericFilterConditionHandler(IFilterConditionHandler nextHandler)
    : AbstractFilterConditionHandler(nextHandler)
{
    protected override string HandleCondition(FilterCondition condition, object? value)
    {
        bool isOperatorValid = condition.Operator.IsValidForType(value.GetType());
        return (condition.Operator.Name, isOperatorValid) switch
        {
            (FilterOperator.EqKey, true) => $"x.{condition.Field} == {value}",
            (FilterOperator.NeKey, true) => $"x.{condition.Field} != {value}",
            (FilterOperator.GteKey, true) => $"x.{condition.Field} >= {value}",
            (FilterOperator.LteKey, true) => $"x.{condition.Field} <= {value}",
            (FilterOperator.GtKey, true) => $"x.{condition.Field} > {value}",
            (FilterOperator.LtKey, true) => $"x.{condition.Field} < {value}",
            _ => throw new NotSupportedException($"The filter condition '{condition.Operator}' is not supported for type '{condition.Value?.GetType().Name}'."),
        };
    }

    protected override bool ShouldHandle(
        FilterCondition condition,
        [NotNullWhen(true)] out object? value)
    {
        value = condition.Value switch
        {
            ushort us => us,
            short s => s,
            uint ui => ui,
            int i => i,
            ulong ul => ul,
            long l => l,
            decimal c => c,
            double d => d,
            float f => f,
            _ => null,
        };
        return value is not null;
    }
}
