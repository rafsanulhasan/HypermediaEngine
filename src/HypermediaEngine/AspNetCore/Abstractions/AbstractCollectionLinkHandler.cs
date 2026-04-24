using Microsoft.AspNetCore.Http;

namespace HypermediaEngine.Abstractions;

public abstract class AbstractCollectionLinkHandler<T>(IHttpContextAccessor httpContextAccessor)
    : AbstractLinkHandler<T, IHypermediaCollectionBuilder<T>>(httpContextAccessor) where T : notnull
{
    internal AbstractCollectionLinkHandler<T> WithHttpContext(HttpContext httpContext)
    {
        HttpContext = httpContext;
        return this;
    }

    internal AbstractCollectionLinkHandler<T> WithBuilder(IHypermediaCollectionBuilder<T> builder)
    {
        Builder = builder;
        return this;
    }
}