namespace HypermediaEngine.Requests.Filtering;

public sealed record class FilterCondition(
    string Field,
    FilterOperator Operator,
    object? Value
);
