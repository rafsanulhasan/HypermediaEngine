using HypermediaEngine.Http;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

using System.Net.Mime;

namespace HypermediaEngine.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public abstract class AbstractProducesHypermediaAttribute<T>
    : Attribute,
      IEndpointFilter,
      IApiResponseMetadataProvider
{
    public Type? Type { get; } = typeof(T);
    public int StatusCode { get; } = StatusCodes.Status200OK;

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        object? result = await next(context).ConfigureAwait(false);

        return result switch
        {
            null => Results.NotFound(),

            //IResult => result,
            //_ when result.GetType().Equals(typeof(Response<>)) => result,
            //_ when result.GetType().Equals(typeof(ListResponse<>)) => result,
            //_ when result is IEnumerable || result.GetType().IsAssignableFrom(typeof(IEnumerable<>))
            //    => result,
            _ when context.HttpContext.Request.Headers.Accept.ToString().Equals(MediaTypeNames.Application.Json, StringComparison.OrdinalIgnoreCase)
                => result,
            _ => TypedResults.Json(
                ProcessHateoas(
                    context,
                    result,
                    context.HttpContext.RequestServices.GetRequiredService<LinkGenerator>()),
                contentType: $"{context.HttpContext.Request.Headers.Accept}; charset=utf-8"),
        };
    }

    protected abstract object? ProcessHateoas(
        EndpointFilterInvocationContext context,
        object result,
        LinkGenerator links);

    public void SetContentTypes(MediaTypeCollection contentTypes)
    {
        contentTypes.Add(HalMediaTypeNames.Application.HalJson);
    }
}
