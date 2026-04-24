using HypermediaEngine.Abstractions;

using Marten.Linq;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HypermediaEngine.Responses.Handlers;

internal sealed class ResponseHandlerResolver<T>(
    IHypermediaObjectBuilder<T> objectBuilder,
    IHypermediaCollectionBuilder<T> collectionBuilder,
    AbstractObjectResponseHandler<T> objectHandler,
    AbstractCollectionResponseHandler<T> collectionHandler,
    IHttpContextAccessor httpContextAccessor,
    IServiceProvider serviceProvider
) : IResponseHandlersResolver<T>
    where T : notnull
{
    internal EndpointFilterInvocationContext? DefaultEndpointFilterInvocationContext { get; set; }

    public async ValueTask<IResponseHandler> ResolveHandler(object response)
    {
        return response switch
        {
            //HypermediaCollectionResponse<T> halCollectionResponse => halCollectionResponse,
            //HypermediaObjectResponse<T> halObjectResponse => halObjectResponse,
            IMartenQueryable<T> or Ok<IMartenQueryable<T>> or JsonHttpResult<IMartenQueryable<T>> when collectionHandler is CollectionResponseHandler<T> handler
                => (await handler.WithQueryParams().ConfigureAwait(false))
                        .WithResponseBuilder(collectionBuilder)
                        .WithEndpointInvocationFilterContext(DefaultEndpointFilterInvocationContext),
            IQueryable<T> or Ok<IQueryable<T>> or JsonHttpResult<IQueryable<T>> when collectionHandler is CollectionResponseHandler<T> handler
                => (await handler.WithQueryParams().ConfigureAwait(false))
                        .WithResponseBuilder(collectionBuilder)
                        .WithEndpointInvocationFilterContext(DefaultEndpointFilterInvocationContext),
            T[] or Ok<T[]> or JsonHttpResult<T[]> when collectionHandler is CollectionResponseHandler<T> handler
                => (await handler.WithHttpContext(httpContextAccessor.HttpContext!).WithQueryParams().ConfigureAwait(false))
                        .WithResponseBuilder(collectionBuilder)
                        .WithEndpointInvocationFilterContext(DefaultEndpointFilterInvocationContext),
            IEnumerable<T> or Ok<IEnumerable<T>> or JsonHttpResult<IEnumerable<T>> when collectionHandler is CollectionResponseHandler<T> handler
                => (await handler.WithHttpContext(httpContextAccessor.HttpContext!).WithQueryParams().ConfigureAwait(false))
                        .WithResponseBuilder(collectionBuilder)
                        .WithEndpointInvocationFilterContext(DefaultEndpointFilterInvocationContext),
            T or Ok<T> or JsonHttpResult<T> when typeof(T).BaseType == typeof(Array)
                && typeof(T).BaseType!.GetElementType() is Type elementType
                && typeof(AbstractCollectionResponseHandler<>).MakeGenericType(elementType) is Type collectionHandlerType
                && serviceProvider.GetService(collectionHandlerType) is IResponseHandler handler
                 => handler,
            T or Ok<T> or JsonHttpResult<T> when objectHandler is TResponseHandler<T> handler
                => handler
                        .WithResponsBuilder(objectBuilder)
                        .WithEndpointInvocationFilterContext(DefaultEndpointFilterInvocationContext),
            _ => throw new InvalidOperationException("Unknown response type"),
        };
    }
}
