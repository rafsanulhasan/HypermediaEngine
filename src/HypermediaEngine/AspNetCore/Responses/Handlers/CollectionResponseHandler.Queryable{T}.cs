using Ardalis.GuardClauses;

using Asp.Versioning;

using HypermediaEngine.Abstractions;
using HypermediaEngine.Attributes;
using HypermediaEngine.Requests;
using HypermediaEngine.Requests.Paging;

using LanguageExt;

using Marten.Linq;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System.Diagnostics.CodeAnalysis;

namespace HypermediaEngine.Responses.Handlers;

internal sealed class CollectionResponseHandler<T>(
    IHttpContextAccessor httpContextAccessor,
    IEnumerable<AbstractCollectionMetadataHandler<T>> metadataHandlers,
    IEnumerable<AbstractCollectionLinkHandler<T>> linkHandlers,
    ILogger<CollectionApiVersionMetadataHandler<T>> logger
) : AbstractCollectionResponseHandler<T>(httpContextAccessor)
    where T : notnull
{
    internal QueryParams? QueryParams { get; private set; }

    public override async ValueTask<object?> HandleCollectionResponse(IEnumerable<T> response)
    {
        Guard.Against.Null(Builder);
        Guard.Against.Null(HttpContext);
        Builder = Builder.WithItems(response, QueryParams);
        ApplyMetadata(response)
            .ApplyLinks();
        object? collectionResponse = response switch
        {
            T[] when typeof(T).IsAssignableFrom(typeof(Array)) => Builder.Build(),
            IEnumerable<T> => Builder.Build(),
            _ when _nextHandler is not null => await _nextHandler.HandleResponseAsync(response).ConfigureAwait(false),
            _ => throw new InvalidOperationException("Next Handler is not available"),
        };
        return collectionResponse;
    }

    public override async ValueTask<object?> HandleQueryableResponse(IQueryable<T> response)
    {
        Guard.Against.Null(Builder);
        if (IsHandled)
        {
            return Response;
        }

        QueryParams ??= new QueryParams();

        Builder = response switch
        {
            IMartenQueryable<T> martenQueryable => await Builder.WithItemsAsync(martenQueryable, QueryParams, CancellationTokenSource.Token).ConfigureAwait(false),
            IQueryable<T> queryable => await Builder.WithItemsAsync(queryable, QueryParams, CancellationTokenSource.Token).ConfigureAwait(false),
            _ => Builder,
        };
        ApplyMetadata(response)
            .ApplyLinks();

        Response = Builder.Build();
        IsHandled = true;
        return response;
    }

    internal async ValueTask<AbstractCollectionResponseHandler<T>> WithQueryParams()
    {
        QueryParams ??= new(null, new OffsetPaging(1, 10));
        OffsetOrCursorPaging paging = new OffsetPaging(1, 10);
        if (HttpContext is not null)
            _ = OffsetOrCursorPaging.TryParse(HttpContext.Request.QueryString.ToString(), out paging);
        QueryParams = QueryParams with
        {
            Paging = paging,
        };
        try
        {
            if (HttpContext is null)
            {
                return this;
            }
            QueryBody? body = await HttpContext.Request.ReadFromJsonAsync<QueryBody>(HttpContext.RequestAborted).ConfigureAwait(false);
            QueryParams = QueryParams with
            {
                Body = body,
            };
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Failed to read QueryParams from the request body. Default QueryParams will be used.");
        }
        return this;
    }

    private CollectionResponseHandler<T> ApplyMetadata(IEnumerable<T> response)
    {
        if (Builder is null)
        {
            throw new InvalidOperationException("Builder is not available.");
        }
        IReadOnlyList<AbstractCollectionMetadataHandler<T>> list = [.. ResolveMetadataHandlers()];
        ListResponseMetadata? metadata = Builder.Metadata;
        foreach (AbstractCollectionMetadataHandler<T> handler in list)
        {
            Builder = handler.Handle(response, metadata);
            metadata = Builder.Metadata;
        }
        return this;
    }

    private CollectionResponseHandler<T> ApplyLinks()
    {
        IReadOnlyList<AbstractCollectionLinkHandler<T>> list = [.. ResolveLinkHandlers()];
        Endpoint endpoint = HttpContext!.GetEndpoint()
                         ?? throw new InvalidOperationException("Endpoint is not available.");
        foreach (AbstractCollectionLinkHandler<T> handler in list)
        {
            Builder = handler.Handle(endpoint.Metadata.GetOrderedMetadata<HateoasStateTransitionAttribute>());
            Builder = handler.Handle(endpoint.Metadata.GetOrderedMetadata<HateoasRelatedAttribute>());
        }
        return this;
    }

    internal override CollectionResponseHandler<T> WithHttpContext(HttpContext httpContext)
    {
        HttpContext = httpContext;
        return this;
    }

    private static Option<string> GetGetApiVersion(
        [NotNull] HttpContext? httpContext,
        [NotNull] IHypermediaCollectionBuilder<T>? builder)
    {
        if (httpContext is null)
        {
            throw new InvalidOperationException("HttpContext is not available.");
        }

        if (builder is null)
        {
            throw new InvalidOperationException("Builder is not available.");
        }

        IServiceProvider sp = httpContext.RequestServices;
        Option<string> apiVersion = sp.GetService<IApiVersionReader>() is IApiVersionReader reader
                   ? reader.Read(httpContext.Request).Distinct(StringComparer.Ordinal).FirstOrDefault() is string version && !string.IsNullOrWhiteSpace(version)
                     ? Option<string>.Some(version)
                     : Option<string>.None
                   : Option<string>.None;
        return apiVersion;
    }

    private IEnumerable<AbstractCollectionMetadataHandler<T>> ResolveMetadataHandlers()
    {
        for (int i = 0; i < metadataHandlers.Count(); i++)
        {
            AbstractCollectionMetadataHandler<T> handler = metadataHandlers.ElementAt(i);
            handler = handler switch
            {
                CollectionApiVersionMetadataHandler<T> apiVersionHandler => apiVersionHandler.WithBuilder(Builder!),
                CollectionETagMetadataHandler<T> etagHandler => etagHandler.WithBuilder(Builder!),
                _ => handler,
            };
            yield return handler;
        }
    }

    private IEnumerable<AbstractCollectionLinkHandler<T>> ResolveLinkHandlers()
    {
        PagingMetadata pagingMetadata = Builder!.Metadata?.Paging
                                     ?? new PagingMetadata()
                                     {
                                         CurrentPage = 1,
                                         PageSize = 10,
                                     };
        for (int i = 0; i < linkHandlers.Count(); i++)
        {
            AbstractCollectionLinkHandler<T> handler = linkHandlers
                .ElementAt(i)
                .WithHttpContext(HttpContext!)
                .WithBuilder(Builder!);

            if (handler is CollectionPageEndpointLinkHandler<T> pageHandler)
            {
                handler = pageHandler.WithMetadata(pagingMetadata);
            }
            yield return handler;
        }
    }
}