using HypermediaEngine.Responses;

namespace HypermediaEngine.Abstractions;

internal abstract class AbstractCollectionMetadataHandler<T>(IHypermediaCollectionBuilder<T> builder)
    : ICollectionMetadataHandler<T> where T : notnull
{
    protected internal IHypermediaCollectionBuilder<T> Builder { get; set; } = builder;
    public abstract IHypermediaCollectionBuilder<T> Handle(IEnumerable<T> result, ListResponseMetadata? metadata = null);
}
