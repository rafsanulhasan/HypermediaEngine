using HypermediaEngine.Abstractions;

using Marten.Linq;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HypermediaEngine.Responses.Handlers;

internal sealed class ResponseHandlerResolver<T>(
    IHypermediaObjectBuilder<T> objectBuilder,
    IHypermediaCollectionBuilder<T> collectionBuilder,
    AbstractObjectResponseHandler<T> objectHandler,
    MartenQueryableCollectionResponseHandler<T> martenHandler,
    QueryableCollectionResponseHandler<T> queryableHandler,
    EnumerableCollectionResponseHandler<T> enumerableHandler,
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
            IMartenQueryable<T> or Ok<IMartenQueryable<T>> or JsonHttpResult<IMartenQueryable<T>>
                => (await martenHandler.WithQueryParams().ConfigureAwait(false))
                        .WithResponseBuilder(collectionBuilder)
                        .WithEndpointInvocationFilterContext(DefaultEndpointFilterInvocationContext),

            IQueryable<T> or Ok<IQueryable<T>> or JsonHttpResult<IQueryable<T>>
                => (await queryableHandler.WithQueryParams().ConfigureAwait(false))
                        .WithResponseBuilder(collectionBuilder)
                        .WithEndpointInvocationFilterContext(DefaultEndpointFilterInvocationContext),

            T[] or Ok<T[]> or JsonHttpResult<T[]>
                => (await enumerableHandler.WithQueryParams().ConfigureAwait(false))
                        .WithResponseBuilder(collectionBuilder)
                        .WithEndpointInvocationFilterContext(DefaultEndpointFilterInvocationContext),

            IEnumerable<T> or Ok<IEnumerable<T>> or JsonHttpResult<IEnumerable<T>>
                => (await enumerableHandler.WithQueryParams().ConfigureAwait(false))
                        .WithResponseBuilder(collectionBuilder)
                        .WithEndpointInvocationFilterContext(DefaultEndpointFilterInvocationContext),

            T or Ok<T> or JsonHttpResult<T>
                when typeof(T).BaseType == typeof(Array)
                  && typeof(T).BaseType!.GetElementType() is Type elementType
                  && typeof(EnumerableCollectionResponseHandler<>).MakeGenericType(elementType) is Type handlerType
                  && serviceProvider.GetService(handlerType) is IResponseHandler elementHandler
                => elementHandler,

            T or Ok<T> or JsonHttpResult<T> when objectHandler is TResponseHandler<T> handler
                => handler
                        .WithResponsBuilder(objectBuilder)
                        .WithEndpointInvocationFilterContext(DefaultEndpointFilterInvocationContext),

            _ => throw new InvalidOperationException("Unknown response type"),
        };
    }
}
