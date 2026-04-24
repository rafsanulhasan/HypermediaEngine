using EntityTagCaching.Abstractions;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

using System.Text;

namespace EntityTagCaching.Middleware;

/// <summary>
/// Middleware that adds ETag headers to HTTP GET responses to enable efficient client-side caching and support
/// conditional requests.
/// </summary>
/// <remarks>This middleware processes only GET requests. It generates an ETag for the response body and compares
/// it to the value of the If-None-Match request header. If the ETag matches, the middleware returns a 304 Not Modified
/// response, allowing clients to avoid downloading unchanged content. For other request methods or when ETag generation
/// is not possible, the middleware passes the request and response through unchanged.</remarks>
/// <param name="next">The delegate representing the next middleware component in the HTTP request processing pipeline.</param>
public sealed class ETagMiddleware(RequestDelegate next)
{
    /// <summary>
    /// Processes an HTTP request within the middleware pipeline, adding an ETag header to successful GET responses and
    /// handling conditional requests based on the ETag value.
    /// </summary>
    /// <remarks>If the request is a GET and the response is successful, this method generates and adds an
    /// ETag header to the response. If the client's ETag matches the generated ETag, the response status is set to 304
    /// (Not Modified) and the response body is not sent. For non-GET requests or unsuccessful responses, the middleware
    /// passes the request through without ETag processing.</remarks>
    /// <param name="context">The HTTP context for the current request, containing information about the request and response.</param>
    /// <returns>A task that represents the asynchronous operation of the middleware.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        if (!HttpMethods.IsGet(context.Request.Method))
        {
            await next(context);
            return;
        }

        Stream originalBody = context.Response.Body;
        await using MemoryStream buffer = new();
        context.Response.Body = buffer;

        try
        {
            await next(context);

            if (context.Response.StatusCode == StatusCodes.Status200OK)
            {
                buffer.Seek(0, SeekOrigin.Begin);
                using StreamReader reader = new(buffer, Encoding.UTF8);
                string responseBody = await reader.ReadToEndAsync();

                var etagService = context.RequestServices.GetService<IETagService>();
                if (etagService is not null 
                 && !string.IsNullOrEmpty(responseBody))
                {
                    string etag = etagService.GenerateETag(responseBody);
                    context.Response.Headers[HeaderNames.ETag] = etag;

                    string ifNoneMatch = context.Request.Headers[HeaderNames.IfNoneMatch].ToString();
                    if (!etagService.IsETagStale(etag, ifNoneMatch))
                    {
                        context.Response.StatusCode = StatusCodes.Status304NotModified;
                        context.Response.Body = originalBody;
                        return;
                    }
                }

                buffer.Seek(0, SeekOrigin.Begin);
                context.Response.Body = originalBody;
                await buffer.CopyToAsync(originalBody);
            }
            else
            {
                buffer.Seek(0, SeekOrigin.Begin);
                context.Response.Body = originalBody;
                await buffer.CopyToAsync(originalBody);
            }
        }
        finally
        {
            context.Response.Body = originalBody;
        }
    }
}
