using Microsoft.AspNetCore.Http;

namespace SynergyFx.HypermediaEngine.Abstractions;

public abstract class AbstractLinkHandler<T, TBuilder>(IHttpContextAccessor httpContextAccessor)
    : ILinkHandler<T, TBuilder> where T : notnull
    where TBuilder : IHypermediaBuilder<T>
{
    protected internal HttpContext? HttpContext { get; set; } = httpContextAccessor.HttpContext;
    protected internal TBuilder? Builder { get; set; }

    public abstract TBuilder Handle(IEnumerable<HateoasLinkAttribute> attributes);
}
