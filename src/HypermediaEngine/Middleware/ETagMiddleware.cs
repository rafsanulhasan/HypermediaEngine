namespace HypermediaEngine.Middleware;

using HypermediaEngine.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Text;

public class ETagMiddleware
{
    private readonly RequestDelegate _next;

    public ETagMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!HttpMethods.IsGet(context.Request.Method))
        {
            await _next(context);
            return;
        }

        var originalBody = context.Response.Body;
        using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        try
        {
            await _next(context);

            if (context.Response.StatusCode == StatusCodes.Status200OK)
            {
                buffer.Seek(0, SeekOrigin.Begin);
                var responseBody = await new StreamReader(buffer).ReadToEndAsync();

                var etagService = context.RequestServices.GetService<IETagService>();
                if (etagService != null && !string.IsNullOrEmpty(responseBody))
                {
                    var etag = etagService.GenerateETag(responseBody);
                    context.Response.Headers["ETag"] = etag;

                    var ifNoneMatch = context.Request.Headers["If-None-Match"].ToString();
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
