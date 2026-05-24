using SynergyFx.HypermediaEngine.Abstractions;

namespace SynergyFx.HypermediaEngine.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class HateoasStateTransitionAttribute(
    string name,
    string rel,
    string method,
    string routeName)
    : HateoasLinkAttribute(name, rel, method, routeName)
{
}
