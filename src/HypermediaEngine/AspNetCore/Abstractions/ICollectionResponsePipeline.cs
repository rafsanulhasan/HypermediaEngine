using HypermediaEngine.Responses;

namespace HypermediaEngine.Abstractions;

internal interface ICollectionResponsePipeline<T>
    where T : notnull
{
    ValueTask<CollectionResponseContext<T>> Process(
        CollectionResponseContext<T> context);
}
