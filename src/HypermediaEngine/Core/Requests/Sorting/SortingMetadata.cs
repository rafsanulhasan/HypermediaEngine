namespace HypermediaEngine.Requests.Sorting;

public sealed record class SortingMetadata(
    string Field,
    SortDirection Direction
);
