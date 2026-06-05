namespace SynergyFx.HypermediaEngine.Requests.Filtering;

public interface IFilterConditionHandler
{
    string Handle(FilterCondition condition);
}
