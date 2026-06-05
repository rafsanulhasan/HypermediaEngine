using SynergyFx.HypermediaEngine.Requests.Sorting;

namespace SynergyFx.HypermediaEngine.Responses.Metadata;

public sealed record class SortingMetadata(
    string Field,
    SortDirection Direction
);
