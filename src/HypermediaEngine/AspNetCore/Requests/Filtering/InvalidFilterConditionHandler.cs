namespace SynergyFx.HypermediaEngine.Requests.Filtering;

internal sealed class InvalidFilterConditionHandler : IFilterConditionHandler
{
    public string Handle(FilterCondition condition)
    {
        throw new NotSupportedException($"The filter operator '{condition.Operator}' is not supported for type '{condition.Value?.GetType().Name}'.");
    }
}
