using Asp.Versioning;

using HypermediaEngine.Abstractions;
using HypermediaEngine.AspNetCore.Responses.Handlers;
using HypermediaEngine.Helpers;
using HypermediaEngine.Http;
using HypermediaEngine.Responses.Handlers;

using LanguageExt;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

using System.Net.Mime;

namespace HypermediaEngine.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ProducesHalResponseAttribute<T, TResponse>
    : Attribute, IEndpointFilter
    where T : notnull
    where TResponse : AbstractResponse<T>
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        object? result = await next(context).ConfigureAwait(false);

        if (result is null)
        {
            return TypedResults.NotFound(result);
        }

        if (result is StatusCodeHttpResult statusCodeHttpResult)
        {
            context.HttpContext.Response.StatusCode = statusCodeHttpResult.StatusCode;
            return statusCodeHttpResult;
        }

        StringValues acceptHeader = context.HttpContext.GetRequestHeaderOrDefault(HeaderNames.Accept, HalMediaTypeNames.Application.HalJson);
        string mediaType = acceptHeader.GetMediaTypeOrDefault(HalMediaTypeNames.Application.HalJson);
        string contentType = $"{mediaType}; charset=utf-8";
        object? resultObj = ProcessHateoas(
            context,
            result,
            context.HttpContext.RequestServices.GetRequiredService<LinkGenerator>());

        JsonOptions? jsonOptions = context.HttpContext.RequestServices.GetService<JsonOptions>();

        if (resultObj is null)
        {
            return TypedResults.NotFound(resultObj);
        }

        Option<string> apiVersion = GetApiVersion(context.HttpContext);

        resultObj = resultObj switch
        {
            JsonHttpResult<T> jsonOfT when acceptHeader.Equals(MediaTypeNames.Application.Json)
                => TypedResults.Json(
                    jsonOfT.Value,
                    jsonOptions?.SerializerOptions,
                    contentType: HalMediaTypeNames.AppendVersionToMediaType(contentType, apiVersion)),
            JsonHttpResult<TResponse> jsonOfTResponse when acceptHeader.Equals(MediaTypeNames.Application.Json)
                => TypedResults.Json(
                    jsonOfTResponse.Value!.Data,
                    jsonOptions?.SerializerOptions,
                    contentType: HalMediaTypeNames.AppendVersionToMediaType(contentType, apiVersion)),
            JsonHttpResult<TResponse> jsonOfTResponseHal when acceptHeader.Equals(HalMediaTypeNames.Application.HalJson.Name)
                => TypedResults.Json(
                    jsonOfTResponseHal.Value,
                    jsonOptions?.SerializerOptions,
                    contentType: HalMediaTypeNames.AppendVersionToMediaType(contentType, apiVersion)),
            _ => TypedResults.Json(
                resultObj,
                jsonOptions?.SerializerOptions,
                contentType: HalMediaTypeNames.AppendVersionToMediaType(contentType, apiVersion)),
        };

        return resultObj;
    }

    private static async Task<object?> ProcessHateoas(
        EndpointFilterInvocationContext context,
        object result,
        LinkGenerator links)
    {
        HttpContext http = context.HttpContext;
        IServiceProvider sp = http.RequestServices;
        IResponseHandlersResolver<T> responseHandlerReolver = sp.GetRequiredService<IResponseHandlersResolver<T>>();
        IResponseHandler responseHandler = await responseHandlerReolver.ResolveHandler(result).ConfigureAwait(false);
        using IResponseHandler tObjectResponseHandler = sp.GetRequiredKeyedService<IResponseHandler>(ObjectResponseHandlers.TResponseHandlerKey);
        using IResponseHandler objectResponseHandler = sp.GetRequiredKeyedService<SimpleObjectResponseHandler>(ObjectResponseHandlers.ObjectResponseHandler);
        responseHandler = responseHandler switch
        {
            CollectionResponseHandler<T> collectionResponseHandler => collectionResponseHandler
                                                                        .WithEndpointInvocationFilterContext(context)
                                                                        .WithNextHandler(tObjectResponseHandler),
            TResponseHandler<T> tResponseHandler => tResponseHandler
                                                        .WithEndpointInvocationFilterContext(context)
                                                        .WithNextHandler(objectResponseHandler),
            _ => responseHandler,
        };
        object? response = await responseHandler.HandleResponseAsync(result).ConfigureAwait(false);
        responseHandler.Dispose();
        return response;
    }

    private static Option<string> GetApiVersion(HttpContext httpContext)
    {
        IApiVersionReader? reader = httpContext.RequestServices.GetService<IApiVersionReader>();
        if (reader is null)
        {
            return Option<string>.None;
        }
        string? apiVersion = reader
            .Read(httpContext.Request)
            .Distinct(StringComparer.Ordinal)
            .FirstOrDefault();
        return string.IsNullOrWhiteSpace(apiVersion)
             ? Option<string>.None
             : Option<string>.Some(apiVersion);
    }
}
