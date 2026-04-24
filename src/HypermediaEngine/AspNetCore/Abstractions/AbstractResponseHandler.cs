using HypermediaEngine.Responses;

using Microsoft.AspNetCore.Http;

namespace HypermediaEngine.Abstractions;

public abstract class AbstractObjectResponseHandler<T>
    : IResponseHandler where T : notnull
{
    protected IResponseHandler? _nextHandler;
    private bool _disposedValue;

    protected AbstractObjectResponseHandler(
        IHttpContextAccessor httpContextAccessor,
        IResponseHandler? nextHandler = null)
    {
        HttpContext = httpContextAccessor.HttpContext;
        _nextHandler = nextHandler;
    }

    protected internal EndpointFilterInvocationContext? EndpointFilterInvocationContext { get; set; }
    protected internal HttpContext? HttpContext { get; set; }
    protected internal IHypermediaBuilder<T>? ObjectBuilder { get; set; }
    protected internal ObjectResponseMetadata? Meta { get; set; }
    protected internal bool IsHandled { get; set; }
    protected internal object? Response { get; set; }

    protected CancellationTokenSource CancellationTokenSource => HttpContext?.RequestAborted is not null
        ? CancellationTokenSource.CreateLinkedTokenSource(HttpContext.RequestAborted)
        : new CancellationTokenSource();

    public async ValueTask<object?> HandleResponseAsync(object? response)
    {
        object? result = response switch
        {
            T typedResponse => await HandleTypedResponse(typedResponse).ConfigureAwait(false),
            _ when _nextHandler is not null => await _nextHandler.HandleResponseAsync(response).ConfigureAwait(false),
            _ => await ValueTask.FromResult(response).ConfigureAwait(false),
        };
        return result;
    }

    public abstract ValueTask<object?> HandleTypedResponse(T response);

    /// <inheritdoc />
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    internal AbstractObjectResponseHandler<T> WithHttpContext(HttpContext httpContext)
    {
        HttpContext = httpContext;
        return this;
    }

    internal AbstractObjectResponseHandler<T> WithResponsBuilder(IHypermediaBuilder<T> builder)
    {
        ObjectBuilder = builder;
        return this;
    }

    internal AbstractObjectResponseHandler<T> WithMeta(ObjectResponseMetadata meta)
    {
        Meta = meta;
        return this;
    }

    internal AbstractObjectResponseHandler<T> WithEndpointInvocationFilterContext(EndpointFilterInvocationContext? context)
    {
        if (context is null)
        {
            return this;
        }
        EndpointFilterInvocationContext = context;
        HttpContext = context.HttpContext;
        return this;
    }

    internal AbstractObjectResponseHandler<T> WithNextHandler(IResponseHandler nextHandler)
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
