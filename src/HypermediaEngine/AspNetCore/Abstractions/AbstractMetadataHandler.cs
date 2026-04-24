using HypermediaEngine.Responses;

namespace HypermediaEngine.Abstractions;

internal abstract class AbstractObjectMetadataHandler<T>
    : IObjectMetadataHandler<T> where T : notnull
{
    protected internal IHypermediaObjectBuilder<T>? Builder { get; set; }

    public abstract IHypermediaObjectBuilder<T> Handle(T? result, ObjectResponseMetadata? metadata = null);
    internal AbstractObjectMetadataHandler<T> WithBuilder(IHypermediaObjectBuilder<T> builder)
    {
        Builder = builder;
        return this;
    }
}
