namespace HypermediaEngine.Models;

public class HypermediaResponse<T>
{
    public T? Data { get; set; }
    public Dictionary<string, HypermediaLink> Links { get; set; } = new();
    public Dictionary<string, object?>? Metadata { get; set; }
}
