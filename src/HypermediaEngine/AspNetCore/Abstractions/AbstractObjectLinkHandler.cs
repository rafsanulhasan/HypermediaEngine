using Microsoft.AspNetCore.Http;

namespace HypermediaEngine.Abstractions;

public abstract class AbstractObjectLinkHandler<T>(IHttpContextAccessor httpContextAccessor)    
    : AbstractLinkHandler<T, IHypermediaObjectBuilder<T>>(httpContextAccessor) where T : notnull
{
    internal AbstractObjectLinkHandler<T> WithHttpContext(HttpContext httpContext)
    {
        HttpContext = httpContext;
        return this;
    }

    internal AbstractObjectLinkHandler<T> WithBuilder(IHypermediaObjectBuilder<T> builder)
    {
        Builder = builder;
        return this;
    }
}
