namespace HypermediaEngine;

public class HypermediaEngineOptions
{
    public bool EnableETagCaching { get; set; } = true;
    public string DefaultMediaType { get; set; } = "application/json";
}
