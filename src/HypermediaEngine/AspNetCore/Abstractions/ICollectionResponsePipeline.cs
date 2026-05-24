using SynergyFx.HypermediaEngine.Responses;

namespace SynergyFx.HypermediaEngine.Abstractions;

internal interface ICollectionResponsePipeline<T>
    where T : notnull
{
    ValueTask<CollectionResponseContext<T>> Process(
        CollectionResponseContext<T> context);
}
