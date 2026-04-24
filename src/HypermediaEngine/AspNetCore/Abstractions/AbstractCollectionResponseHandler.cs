using Asp.Versioning;

using HypermediaEngine.Responses;

using LanguageExt;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;

namespace HypermediaEngine.Abstractions;

public abstract class AbstractCollectionResponseHandler<T>
    : IResponseHandler where T : notnull
{
    protected IResponseHandler? _nextHandler;
    private bool _disposedValue;

    protected AbstractCollectionResponseHandler(
        IHttpContextAccessor httpContextAccessor,
        IResponseHandler? nextHandler = null)
    {
        HttpContext = httpContextAccessor.HttpContext;
        _nextHandler = nextHandler;
    }

    protected internal EndpointFilterInvocationContext? EndpointFilterInvocationContext { get; set; }
    protected internal HttpContext? HttpContext { get; set; }
    protected internal IHypermediaCollectionBuilder<T>? Builder { get; set; }
    protected internal ListResponseMetadata? Meta { get; set; }

    protected internal bool IsHandled { get; set; }
    protected internal object? Response { get; set; }

    protected CancellationTokenSource CancellationTokenSource => HttpContext?.RequestAborted is not null
        ? CancellationTokenSource.CreateLinkedTokenSource(HttpContext.RequestAborted)
        : new CancellationTokenSource();

    public async ValueTask<object?> HandleResponseAsync(object? response)
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
        object? result = response switch
        {
            T[] array when response.GetType().IsAssignableTo(typeof(Array)) => await HandleCollectionResponse(array).ConfigureAwait(false),
            IEnumerable<T> typedResponse => await HandleCollectionResponse(typedResponse).ConfigureAwait(false),
            Ok<IEnumerable<T>> okTypedResponse => await HandleCollectionResponse(okTypedResponse.Value).ConfigureAwait(false),
            JsonHttpResult<IEnumerable<T>> jsonHttpResult => await HandleCollectionResponse(jsonHttpResult.Value).ConfigureAwait(false),
            _ when _nextHandler is not null => await _nextHandler.HandleResponseAsync(response).ConfigureAwait(false),
            _ => response,
        };
        return result;
    }

    public abstract ValueTask<object?> HandleCollectionResponse(IEnumerable<T> response);
    public abstract ValueTask<object?> HandleQueryableResponse(IQueryable<T> response);

    internal ValueTask<object?> HandleOkTypedResponse(Ok<IEnumerable<T>> response)
    {
        return response is { Value: IEnumerable<T> typedValue }
            ? HandleCollectionResponse(typedValue)
            : throw new ArgumentNullException(nameof(response));
    }

    internal ValueTask<object?> HandleJsonTypedResponse(JsonHttpResult<IEnumerable<T>> response)
    {
        return response is { Value: IEnumerable<T> typedValue }
            ? HandleCollectionResponse(typedValue)
            : throw new ArgumentNullException(nameof(response));
    }

    internal ValueTask<object?> HandleOkTypedResponse(Ok<IQueryable<T>> response)
    {
        return response is { Value: IQueryable<T> typedValue }
            ? HandleQueryableResponse(typedValue)
            : throw new ArgumentNullException(nameof(response));
    }

    internal ValueTask<object?> HandleJsonTypedResponse(JsonHttpResult<IQueryable<T>> response)
    {
        return response is { Value: IQueryable<T> typedValue }
            ? HandleQueryableResponse(typedValue)
            : throw new ArgumentNullException(nameof(response));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    internal virtual AbstractCollectionResponseHandler<T> WithHttpContext(HttpContext httpContext)
    {
        HttpContext = httpContext;
        return this;
    }

    internal AbstractCollectionResponseHandler<T> WithResponseBuilder(IHypermediaCollectionBuilder<T> builder)
    {
        Builder = builder;
        return this;
    }

    internal AbstractCollectionResponseHandler<T> WithMeta(ListResponseMetadata meta)
    {
        Meta = meta;
        return this;
    }

    internal AbstractCollectionResponseHandler<T> WithEndpointInvocationFilterContext(EndpointFilterInvocationContext? context)
    {
        if (context is null)
        {
            return this;
        }
        EndpointFilterInvocationContext = context;
        HttpContext = context.HttpContext;
        return this;
    }

    internal AbstractCollectionResponseHandler<T> WithNextHandler(IResponseHandler nextHandler)
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
