using Asp.Versioning;

using EntityTagCaching.Models;

using HypermediaEngine.Abstractions;
using HypermediaEngine.Attributes;
using HypermediaEngine.Http;

using LanguageExt;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System.Collections.Immutable;

namespace HypermediaEngine.Responses.Handlers;

#pragma warning disable MA0048 // File name must match type name
internal sealed class TResponseHandler<T>(
    IHttpContextAccessor httpContextAccessor,
    IHypermediaObjectBuilder<T> builder,
    IResponseHandler nexthandler,
    ILogger<TResponseHandler<T>> logger
) : AbstractObjectResponseHandler<T>(httpContextAccessor, nexthandler) where T : notnull
{
    public override async ValueTask<object?> HandleTypedResponse(T response)
    {
        if (HttpContext is null)
        {
            throw new InvalidOperationException("HttpContext is not available.");
        }
        IServiceProvider sp = HttpContext.RequestServices;
        Option<string> apiVersion = sp.GetService<IApiVersionReader>() is IApiVersionReader reader
                                  ? reader.Read(HttpContext.Request).Distinct(StringComparer.Ordinal).FirstOrDefault() is string version && !string.IsNullOrWhiteSpace(version)
                                    ? Option<string>.Some(version)
                                    : Option<string>.None
                                  : Option<string>.None;
        
        builder = builder.WithData(response);

        Endpoint endpoint = HttpContext.GetEndpoint()
                         ?? throw new InvalidOperationException("No endpoint found in the current context.");
        ImmutableHashSet<AbstractObjectLinkHandler<T>> linkHandlers = ResolveLinkHandlers(HttpContext, builder, sp);

        foreach (AbstractObjectLinkHandler<T> handler in linkHandlers)
        {
            builder = handler.Handle(endpoint.Metadata.GetOrderedMetadata<HateoasStateTransitionAttribute>());
            builder = handler.Handle(endpoint.Metadata.GetOrderedMetadata<HateoasRelatedAttribute>());
        }

        ObjectResponseMetadata objectMeta = new (EntityTag.Empty);

        foreach (IObjectMetadataHandler<T> handler in ResolveObjectMetadataHandlers(builder, sp))
        {
            builder = handler.Handle(response, objectMeta);
        }

        HypermediaObjectResponse<T> halResponse = builder.Build();
        JsonHttpResult<HypermediaObjectResponse<T>> jsonHttpResult = TypedResults.Json(
            halResponse, 
            contentType: HalMediaTypeNames.AppendVersionToMediaType($"{HalMediaTypeNames.Application.HalJson}; charset=utf-8", apiVersion),
            statusCode: StatusCodes.Status200OK);

        return await ValueTask.FromResult(jsonHttpResult).ConfigureAwait(false);
    }

    private ImmutableHashSet<IObjectMetadataHandler<T>> ResolveObjectMetadataHandlers(
        IHypermediaObjectBuilder<T> builder,        
        IServiceProvider serviceProvider)
    {
        if (HttpContext is null)
        {
            throw new InvalidOperationException("HttpContext is not available.");
        }
        ILogger<ObjectEtagMetadataHandler<T>> logger = serviceProvider.GetRequiredService<ILogger<ObjectEtagMetadataHandler<T>>>();
        ImmutableHashSet<IObjectMetadataHandler<T>> handlers = 
        [
            .. serviceProvider
                .GetServices<AbstractObjectMetadataHandler<T>>()
                .Select(h => h.WithBuilder(builder)),
        ];
        return handlers;
    }

    private ImmutableHashSet<AbstractObjectLinkHandler<T>> ResolveLinkHandlers(
        HttpContext httpContext,
        IHypermediaObjectBuilder<T> builder,
        IServiceProvider sp)
    {
        return [.. sp
            .GetServices<AbstractObjectLinkHandler<T>>()
            .Select(h => h.WithHttpContext(httpContext).WithBuilder(builder)),
        ];
    }
}
#pragma warning restore MA0048 // File name must match type name
