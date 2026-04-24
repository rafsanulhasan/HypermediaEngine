using EntityTagCaching.Middleware;

using Microsoft.AspNetCore.Builder;

namespace EntityTagCaching;

/// <summary>
/// Provides extension methods for configuring the application request pipeline in ASP.NET Core.
/// </summary>
/// <remarks>This class contains methods that extend the functionality of the IApplicationBuilder interface,
/// enabling the addition of custom middleware components to the application's request processing pipeline.</remarks>
public static class ApplicationBuilderHelpers
{
    /// <summary>
    /// Adds middleware to the application's request pipeline that enables ETag-based HTTP caching for responses.
    /// </summary>
    /// <remarks>This middleware allows clients to make conditional requests using ETags, which can reduce
    /// bandwidth usage and improve performance by preventing unnecessary data transfers when resources have not
    /// changed.</remarks>
    /// <param name="app">The application builder used to configure the middleware pipeline. Cannot be null.</param>
    /// <returns>The same instance of the application builder, to allow for method chaining.</returns>
    public static IApplicationBuilder UseETagCaching(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ETagMiddleware>();
    }
}
