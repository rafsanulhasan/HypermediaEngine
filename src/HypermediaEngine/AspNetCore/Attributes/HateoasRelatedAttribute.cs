using HypermediaEngine.Abstractions;

namespace HypermediaEngine.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class HateoasRelatedAttribute(
    string name,
    string rel,
    string method,
    string routeName)
    : HateoasLinkAttribute(name, rel, method, routeName)
{
}
