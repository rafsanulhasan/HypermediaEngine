using Microsoft.Extensions.Logging;

namespace SynergyFx.HypermediaEngine.Requests.Filtering;

internal sealed class FilterNodeHandler(
    IFilterConditionHandler conditionHandler,
    ILogger<FilterNodeHandler> logger
) : IFilterNodeHandler
{
    public string Handle(FilterNode node)
    {
        int localConditionsCount = node.Conditions?.Count ?? 0;
        int childrenCount = node.Children?.Count ?? 0;
        int totalElements = localConditionsCount + childrenCount;

        if (node.Logic is null && totalElements > 1)
        {
            throw new InvalidOperationException("Logic operator (And/Or) must be specified when combining multiple conditions or child nodes.");
        }
        if (node.Logic is not null && totalElements == 0)
        {
            throw new InvalidOperationException("Conditions or Children is required when a Logic operator is provided.");
        }

        List<string> parts = [];
        if (node.Conditions is not null)
        {
            foreach (FilterCondition condition in node.Conditions)
            {
                try
                {
                    string conditionString = conditionHandler.Handle(condition);
                    parts.Add(conditionString);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(
                        ex,
                        "Failed to handle filter condition: {Condition}. Skipping this condition.",
                        condition);
                }
            }
        }

        if (node.Children is not null)
        {
            foreach (FilterNode child in node.Children)
            {
                try
                {
                    string childString = Handle(child);
                    if (string.IsNullOrWhiteSpace(childString))
                    {
                        continue;
                    }
                    int childElements = (child.Conditions?.Count ?? 0) + (child.Children?.Count ?? 0);
                    bool needsParentheses = child.Logic != node.Logic
                                         && childElements > 1;
                    parts.Add(needsParentheses ? $"({childString})" : childString);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(
                        ex,
                        "Failed to handle filter child node: {Child}. Skipping this child node.",
                        child);
                }
            }
        }

        if (parts.Count == 0)
        {
            return string.Empty;
        }

        string logicOperator = $" {(node.Logic ?? FilterLogic.And).Operator} ";

        string result = string.Join(logicOperator, parts);

        return result;
    }
}
