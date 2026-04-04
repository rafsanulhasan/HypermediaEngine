namespace HypermediaEngine.Extensions;

using HypermediaEngine.Middleware;
using Microsoft.AspNetCore.Builder;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseETagCaching(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ETagMiddleware>();
    }
}
