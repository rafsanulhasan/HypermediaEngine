using HypermediaEngine.Attributes;
using HypermediaEngine.Requests;
using HypermediaEngine.Requests.Paging;
using HypermediaEngine.Responses;
using HypermediaEngine.Responses.Handlers;
using HypermediaEngine.Responses.Metadata;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HypermediaEngine.Abstractions;

internal abstract class AbstractCollectionResponseHandler<T, TCollection>(
    IHttpContextAccessor httpContextAccessor,
    IEnumerable<AbstractCollectionMetadataHandler<T>> metadataHandlers,
    IEnumerable<AbstractCollectionLinkHandler<T>> linkHandlers,
    IHypermediaCollectionBuilder<T> builder,
    ICollectionResponseHandler<T>? nextHandler = null
) : ICollectionResponseHandler<T>
    where T : notnull
    where TCollection : IEnumerable<T>
{
    protected ICollectionResponseHandler<T>? _nextHandler = nextHandler;
    private bool _disposedValue;

    protected internal EndpointFilterInvocationContext? EndpointFilterInvocationContext { get; set; }
    protected internal HttpContext? HttpContext { get; set; } = httpContextAccessor.HttpContext;
    protected internal IHypermediaCollectionBuilder<T> Builder { get; set; } = builder;
    protected internal ListResponseMetadata? Meta { get; set; }
    protected internal QueryParams? QueryParams { get; set; }

    protected CancellationTokenSource CancellationTokenSource => HttpContext?.RequestAborted is not null
        ? CancellationTokenSource.CreateLinkedTokenSource(HttpContext.RequestAborted)
        : new CancellationTokenSource();

    public async ValueTask<object?> HandleResponseAsync(object? response)
    {
        return response switch
        {
            null => null,
            TCollection collection => await HandleCollectionResponseAsync(collection).ConfigureAwait(false),
            Ok<TCollection> ok when ok.Value is not null
                => await HandleCollectionResponseAsync(ok.Value).ConfigureAwait(false),
            JsonHttpResult<TCollection> json when json.Value is not null
                => await HandleCollectionResponseAsync(json.Value).ConfigureAwait(false),
             _ when _nextHandler is not null => await _nextHandler.HandleResponseAsync(response).ConfigureAwait(false),
             _ => response,
        };
    }

    public abstract ValueTask<object?> HandleCollectionResponseAsync(TCollection response);

    protected void ApplyMetadata(IEnumerable<T> response, ListResponseMetadata metadata)
    {
        foreach (AbstractCollectionMetadataHandler<T> handler in metadataHandlers)
        {
            Builder = handler.Handle(response, metadata);
        }
    }

    protected void ApplyLinks()
    {
        Endpoint endpoint = HttpContext!.GetEndpoint()
                         ?? throw new InvalidOperationException("Endpoint is not available.");
        PagingMetadata pagingMetadata = Builder!.Metadata?.Paging ?? new PagingMetadata()
        {
            CurrentPage = 1,
            PageSize = 10,
        };
        foreach (AbstractCollectionLinkHandler<T> handler in linkHandlers)
        {
            AbstractCollectionLinkHandler<T> configured = handler
                .WithHttpContext(HttpContext!)
                .WithBuilder(Builder);
            if (configured is CollectionPageEndpointLinkHandler<T> pageHandler)
            {
                configured = pageHandler.WithMetadata(pagingMetadata);
            }
            Builder = configured.Handle(endpoint.Metadata.GetOrderedMetadata<HateoasStateTransitionAttribute>());
            Builder = configured.Handle(endpoint.Metadata.GetOrderedMetadata<HateoasRelatedAttribute>());
        }
    }

    protected async ValueTask<AbstractCollectionResponseHandler<T, TCollection>> WithPopulatedQueryParamsAsync()
    {
        if (HttpContext is null)
        {
            return this;
        }
        try
        {
            QueryBody? body = await HttpContext.Request
                .ReadFromJsonAsync<QueryBody>(HttpContext.RequestAborted)
                .ConfigureAwait(false);
            _ = OffsetOrCursorPaging.TryParse(
                HttpContext.Request.QueryString.ToString(),
                out OffsetOrCursorPaging offsetOrCursorPaging);
            QueryParams = new QueryParams(body: body, paging: offsetOrCursorPaging);
        }
        catch (Exception)
        {
            QueryParams = new(paging: OffsetOrCursorPaging.Default);
        }
        return this;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    internal virtual AbstractCollectionResponseHandler<T, TCollection> WithHttpContext(HttpContext httpContext)
    {
        HttpContext = httpContext;
        return this;
    }

    internal AbstractCollectionResponseHandler<T, TCollection> WithResponseBuilder(IHypermediaCollectionBuilder<T> builder)
    {
        Builder = builder;
        return this;
    }

    internal AbstractCollectionResponseHandler<T, TCollection> WithEndpointInvocationFilterContext(EndpointFilterInvocationContext? context)
    {
        if (context is null)
        {
            return this;
        }
        EndpointFilterInvocationContext = context;
        HttpContext = context.HttpContext;
        return this;
    }

    internal AbstractCollectionResponseHandler<T, TCollection> WithNextHandler(ICollectionResponseHandler<T> nextHandler)
    {
        _nextHandler = nextHandler;
        return this;
    }

    /// <inheritdoc />
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                if (!CancellationTokenSource.IsCancellationRequested)
                {
                    CancellationTokenSource.Cancel();
                }
                CancellationTokenSource.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposedValue = true;
        }
    }
}