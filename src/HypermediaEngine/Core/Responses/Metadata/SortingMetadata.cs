using HypermediaEngine.Requests.Sorting;

namespace HypermediaEngine.Responses.Metadata;

public sealed record class SortingMetadata(
    string Field,
    SortDirection Direction
);
