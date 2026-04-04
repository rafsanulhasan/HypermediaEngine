namespace HypermediaEngine.Models;

public class HypermediaCollectionResponse<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public int TotalCount { get; set; }
    public Dictionary<string, HypermediaLink> Links { get; set; } = new();
    public HypermediaMetadata? Metadata { get; set; }
}
