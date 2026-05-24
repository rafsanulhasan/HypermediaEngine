using Marten.Linq;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SynergyFx.HypermediaEngine.Abstractions;
using SynergyFx.HypermediaEngine.Requests.Filtering;

namespace SynergyFx.HypermediaEngine.Responses.Handlers;

internal sealed class ResponseHandlerResolver<T>(
    IHypermediaObjectBuilder<T> objectBuilder,
    IHypermediaCollectionBuilder<T> collectionBuilder,
    AbstractObjectResponseHandler<T> objectHandler,
    IEnumerable<AbstractCollectionMetadataHandler<T>> collectionMetadataHandlers,
    IEnumerable<AbstractCollectionLinkHandler<T>> collectionLinkHandlers,
    IHttpContextAccessor httpContextAccessor,
    IFilterNodeHandler filterNodeHandler,
    IServiceProvider serviceProvider
) : IResponseHandlersResolver<T>
    where T : notnull
{
    internal EndpointFilterInvocationContext? DefaultEndpointFilterInvocationContext { get; set; }

    public ValueTask<IResponseHandler> ResolveHandler(object response)
    {
        ICollectionResponsePipeline<T> martenQueryableResponsePipeline = new MartenQueryableFilteringRule<T>(
            new MartenQueryablePagingRule<T>(
                new MartenQueryableSortingRule<T>(
                    new MartenQueryableFinalPagingRule<T>()
                )
            )
        );
        ICollectionResponsePipeline<T> queryableResponsePipeline = new QueryableFilteringRule<T>(
            new QueryablePagingRule<T>(
                new QueryableSortingRule<T>(
                    new QueryableFinalPagingRule<T>()
                )
            )
        );
        ICollectionResponsePipeline<T> enumerableResponsePipeline = new EnumerableQueryParamsProcessingRule<T>(
            new EnumerableFilteringRule<T>(
                new EnumerablePagingRule<T>(
                    new EnumerableSortingRule<T>(
                        new EnumerableFinalPagingRule<T>()
                    )
                ),
                filterNodeHandler
            )
        );
        ICollectionResponseHandler<T> enumerableResponseHandler = new EnumerableCollectionResponseHandler<T>(
                    httpContextAccessor,
                    enumerableResponsePipeline,
                    collectionMetadataHandlers,
                    collectionLinkHandlers,
                    collectionBuilder,
                    serviceProvider.GetRequiredService<ILogger<EnumerableCollectionResponseHandler<T>>>());
        IResponseHandler handler = response switch
        {
            IMartenQueryable<T> or Ok<IMartenQueryable<T>> or JsonHttpResult<IMartenQueryable<T>>
                => new MartenQueryableResponseHandler<T>(
                    httpContextAccessor,
                    martenQueryableResponsePipeline,
                    collectionMetadataHandlers,
                    collectionLinkHandlers,
                    collectionBuilder,
                    serviceProvider.GetRequiredService<ILogger<MartenQueryableResponseHandler<T>>>()),

            IQueryable<T> or Ok<IQueryable<T>> or JsonHttpResult<IQueryable<T>>
                => new QueryableResponseHandler<T>(
                    httpContextAccessor,
                    queryableResponsePipeline,
                    collectionMetadataHandlers,
                    collectionLinkHandlers,
                    collectionBuilder,
                    serviceProvider.GetRequiredService<ILogger<QueryableResponseHandler<T>>>()),

            T[] or Ok<T[]> or JsonHttpResult<T[]> => enumerableResponseHandler,

            IEnumerable<T> or Ok<IEnumerable<T>> or JsonHttpResult<IEnumerable<T>>
                => enumerableResponseHandler,

            T or Ok<T> or JsonHttpResult<T>
                when typeof(T).BaseType == typeof(Array)
                  && typeof(T).BaseType!.GetElementType() is Type elementType
                  && typeof(EnumerableCollectionResponseHandler<>).MakeGenericType(elementType) is Type handlerType
                  && serviceProvider.GetService(handlerType) is IResponseHandler elementHandler
                => elementHandler,

            T or Ok<T> or JsonHttpResult<T> when objectHandler is TResponseHandler<T> objectHandleer
                => objectHandleer
                        .WithResponsBuilder(objectBuilder)
                        .WithEndpointInvocationFilterContext(DefaultEndpointFilterInvocationContext),

            _ => throw new InvalidOperationException("Unknown response type"),
        };

        return ValueTask.FromResult(handler);
    }
}
