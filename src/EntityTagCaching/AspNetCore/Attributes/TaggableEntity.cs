using EntityTagCaching.Models;

using HypermediaEngine.Helpers;
using HypermediaEngine.Http;
using HypermediaEngine.Responses;

using LanguageExt;
using LanguageExt.Common;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

using System.Text.Json;

namespace EntityTagCaching.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class EntityTagCachingAttribute<T> : Attribute, IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);
        Option<EntityTag> requestedEtag = EntityTag.From(context.HttpContext.Request);
        object? value = await next(context).ConfigureAwait(false);
        if (!context.HttpContext.Request.Method.Equals(HttpMethods.Get, StringComparison.OrdinalIgnoreCase))
        {
            return value;
        }

        JsonOptions? jsonOptions = context.HttpContext.RequestServices.GetService<JsonOptions>();
        string acceptheader = context.HttpContext.Request.Headers.Accept.ToString();
        string contentType = $"{acceptheader}; charset=utf-8";
        return value switch
        {
            NotFound<T> notFound => notFound,
            NotFound notFoundNonGeneric => notFoundNonGeneric,
            JsonHttpResult<T> json => await ProcessValue(context, json.Value, requestedEtag, jsonOptions?.SerializerOptions, json.ContentType ?? contentType).ConfigureAwait(false),
            Ok<T> ok => await ProcessValue(context, ok.Value, requestedEtag, jsonOptions?.SerializerOptions, contentType).ConfigureAwait(false),
            _ => value,
        };
    }

    private static async ValueTask<IResult> ProcessValue(
        EndpointFilterInvocationContext context,
        object? value,
        Option<EntityTag> requestedEtag,
        JsonSerializerOptions? jsonOptions,
        string contentType)
    {
        if (value is null)
        {
            return TypedResults.NotFound(value);
        }

        OptionalResult<EntityTag> optionalEtag = EntityTag.Create(value);
        return optionalEtag.Match(
            Some: etag => ProcessEtag(context, etag, requestedEtag, value, jsonOptions, contentType),
            None: () => TypedResults.Json(value, jsonOptions, contentType),
            Fail: ex => TypedResults.Json(value, jsonOptions, contentType));
    }

    private static IResult ProcessEtag(
        EndpointFilterInvocationContext context,
        OptionalResult<EntityTag> optionalEtag,
        Option<EntityTag> requestedEtag,
        object? value,
        JsonSerializerOptions? jsonSerializerOptions,
        string contentType)
    {
        return optionalEtag.Match<IResult>(
            Some: etag =>
            {
                etag
                    .WriteTo(context.HttpContext.Response)
                    .IfSucc(unit => { });

                if (etag.Equals(requestedEtag))
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status304NotModified;
                    return TypedResults.StatusCode(StatusCodes.Status304NotModified);
                }

                return value is T data
                    ? TypedResults.Json(data, jsonSerializerOptions, contentType)
                    : TypedResults.Json(value, jsonSerializerOptions, contentType);
            },
            () => TypedResults.Json(value, jsonSerializerOptions, contentType),
            ex => TypedResults.Json(value, jsonSerializerOptions, contentType));
    }
}

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class EntityTagCachingAttribute<T, TResponse> : Attribute, IEndpointFilter
    where TResponse : AbstractResponse<T>
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);
        StringValues requestedEtagValues = context.HttpContext.Request.Headers.IfNoneMatch;
        Option<EntityTag> requestedEtag = EntityTag.From(requestedEtagValues);
        string version = context.HttpContext.RequestedApiVersion?.ToString()
                      ?? string.Empty;
        object? value = await next(context).ConfigureAwait(false);
        if (!context.HttpContext.Request.Method.Equals(HttpMethods.Get, StringComparison.OrdinalIgnoreCase))
        {
            return value;
        }

        JsonOptions? jsonOptions = context.HttpContext.RequestServices.GetService<JsonOptions>();
        StringValues acceptHeader = context.HttpContext.GetRequestHeaderOrDefault(HeaderNames.Accept, HalMediaTypeNames.Application.HalJson.Name);
        string mediaType = acceptHeader.GetMediaTypeOrDefault(HalMediaTypeNames.Application.HalJson);
        string contentType = $"{mediaType}; charset=utf-8";
        return value switch
        {
            NotFound<TResponse> notFound => notFound,
            NotFound notFoundNonGeneric => notFoundNonGeneric,
            JsonHttpResult<TResponse> json => await ProcessValue(context, json.Value, requestedEtag, jsonOptions?.SerializerOptions, json.ContentType ?? contentType).ConfigureAwait(false),
            Ok<TResponse> ok => await ProcessValue(context, ok.Value, requestedEtag, jsonOptions?.SerializerOptions, contentType).ConfigureAwait(false),
            _ => value,
        };
    }

    private static async ValueTask<IResult> ProcessValue(
        EndpointFilterInvocationContext context,
        object? value,
        Option<EntityTag> requestedEtag,
        JsonSerializerOptions? jsonOptions,
        string contentType)
    {
        if (value is null)
        {
            return TypedResults.NotFound(value);
        }
        OptionalResult<EntityTag> optionalEtag = EntityTag.Create(value);
        return await optionalEtag
            .Match(
                Some: etag => ProcessEtag(context, etag, requestedEtag, value, jsonOptions, contentType),
                None: () => ValueTask.FromResult<IResult>(TypedResults.Json(value, jsonOptions, contentType)),
                Fail: ex => ValueTask.FromResult<IResult>(TypedResults.Json(value, jsonOptions, contentType)))
            .ConfigureAwait(false);
    }

    private static async ValueTask<IResult> ProcessEtag(
        EndpointFilterInvocationContext context,
        OptionalResult<EntityTag> optionalEtag,
        Option<EntityTag> requestedEtag,
        object? value,
        JsonSerializerOptions? jsonSerializerOptions,
        string contentType)
    {
        return await optionalEtag
            .Match(async etag =>
            {
                if (etag == requestedEtag
                 && contentType.Contains(HalMediaTypeNames.Application.HalJson, StringComparison.OrdinalIgnoreCase))
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status304NotModified;
                    return TypedResults.StatusCode(StatusCodes.Status304NotModified);
                }

                if (value is not TResponse response)
                {
                    return TypedResults.Json(value, contentType: contentType);
                }

                OptionalResult<EntityTag> dataEtagResult = EntityTag.Create(response.Data);
                EntityTag dataEtag = dataEtagResult.Match(
                    tag => tag,
                    () => etag,
                    _ => etag);

                if (contentType.Contains(HalMediaTypeNames.Application.HalJson, StringComparison.OrdinalIgnoreCase))
                {
                    context.HttpContext.Response.Headers.ETag = etag;

                    TResponse newResponse = response with
                    {
                        Meta = (response.Meta ?? new ObjectResponseMetadata(dataEtag)) with
                        {
                            EntityTag = dataEtag,
                        },
                    };
                    return TypedResults.Json(newResponse, jsonSerializerOptions, contentType);
                }

                context.HttpContext.Response.Headers.ETag = dataEtag;
                if (dataEtag == requestedEtag)
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status304NotModified;
                    return TypedResults.StatusCode(StatusCodes.Status304NotModified);
                }

                return TypedResults.Json(response, jsonSerializerOptions, contentType: contentType);
            },
            () => Task.FromResult<IResult>(TypedResults.Json(value, jsonSerializerOptions, contentType: contentType)),
             _ => Task.FromResult<IResult>(TypedResults.Json(value, jsonSerializerOptions, contentType: contentType)))
            .ConfigureAwait(false);
    }
}
