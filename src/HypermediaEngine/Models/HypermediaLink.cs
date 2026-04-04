namespace HypermediaEngine.Models;

public class HypermediaLink
{
    public string Href { get; set; } = string.Empty;
    public string Method { get; set; } = "GET";
    public string? Title { get; set; }
    public string? Type { get; set; }
}
