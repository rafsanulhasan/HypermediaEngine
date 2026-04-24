using Asp.Versioning;

using HypermediaEngine.Abstractions;
using HypermediaEngine.Helpers;
using HypermediaEngine.Responses.Handlers;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HypermediaEngine.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class ProducesHalAttribute<T>
    : Attribute, IEndpointFilter
    where T : notnull
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        object? result = await next(context).ConfigureAwait(false);
        StringValues acceptHeader = context.HttpContext.GetRequestHeaderOrDefault(
            HeaderNames.Accept.ToLowerInvariant(),
            MediaTypeNames.Application.Json);
        string mediaType = acceptHeader.GetMediaTypeOrDefault(MediaTypeNames.Application.Json);

        string version = context.HttpContext.RequestedApiVersion is ApiVersion apiVersion
                       ? apiVersion.ToString()
                       : string.Empty;
        string contentType = $"{mediaType}; charset=utf-8";

        bool shouldReturnPlainJson = string.IsNullOrWhiteSpace(mediaType)
                                  || mediaType.StartsWith(MediaTypeNames.Application.Json, StringComparison.OrdinalIgnoreCase);

        JsonSerializerOptions jsonSerializerOptions = context.HttpContext.RequestServices.GetService<JsonOptions>()?.SerializerOptions
                                                   ?? context.HttpContext.RequestServices.GetService<JsonSerializerOptions>()
                                                   ?? new()
                                                   {
                                                       WriteIndented = false,
                                                       DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                                                       PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                                                   };


        return result switch
        {
            null => TypedResults.NotFound(),
            _ when shouldReturnPlainJson => result,
            _ => TypedResults.Json(
                    await ProcessHateoas(
                        context,
                        result
                    ).ConfigureAwait(false),
                    options: jsonSerializerOptions,
                    contentType),
        };
    }

    private static async Task<object?> ProcessHateoas(
        EndpointFilterInvocationContext context,
        object result)
    {
        HttpContext http = context.HttpContext;
        IServiceProvider sp = http.RequestServices;
        IResponseHandlersResolver<T> responseHandlerReolver = sp.GetRequiredService<IResponseHandlersResolver<T>>();
        IResponseHandler responseHandler = await responseHandlerReolver.ResolveHandler(result).ConfigureAwait(false);
        using IResponseHandler tObjectResponseHandler = sp.GetRequiredService<AbstractObjectResponseHandler<T>>();
        using IResponseHandler objectResponseHandler = sp.GetRequiredService<IResponseHandler>();
        object? response = responseHandler switch
        {
            CollectionResponseHandler<T> collectionResponseHandler => await collectionResponseHandler
                                                                            .WithEndpointInvocationFilterContext(context)
                                                                            .WithHttpContext(http)
                                                                            .HandleResponseAsync(result)
                                                                            .ConfigureAwait(false),
            TResponseHandler<T> tResponseHandler => await tResponseHandler
                                                        .WithEndpointInvocationFilterContext(context)
                                                        .WithHttpContext(http)
                                                        .WithNextHandler(objectResponseHandler)
                                                        .HandleResponseAsync(result)
                                                        .ConfigureAwait(false),
            SimpleObjectResponseHandler simpleObjectResponseHandler => await simpleObjectResponseHandler
                                                                            .WithEndpointInvocationFilterContext(context)
                                                                            .WithHttpContext(http)
                                                                            .HandleResponseAsync(result)
                                                                            .ConfigureAwait(false),
            _ => await responseHandler.HandleResponseAsync(result).ConfigureAwait(false),
        };
        responseHandler.Dispose();
        return response;
    }
}
