namespace HypermediaEngine.Abstractions;

public abstract class HateoasLinkAttribute(
    string name,
    string rel,
    string method,
    string routeName
) : Attribute
{
    public string Name { get; } = name;
    public string Rel { get; } = rel;
    public string Method { get; } = method;

    public string RouteName { get; } = routeName;
}
