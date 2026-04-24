using HypermediaEngine.Responses;

namespace HypermediaEngine.Abstractions;

internal abstract class AbstractCollectionMetadataHandler<T>
    : ICollectionMetadataHandler<T> where T : notnull
{
    protected internal IHypermediaCollectionBuilder<T>? Builder { get; set; }
    public abstract IHypermediaCollectionBuilder<T> Handle(IEnumerable<T> result, ListResponseMetadata? metadata = null);

    internal AbstractCollectionMetadataHandler<T> WithBuilder(IHypermediaCollectionBuilder<T> builder)
    {
        Builder = builder;
        return this;
    }
}
