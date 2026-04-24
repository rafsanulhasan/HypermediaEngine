using HypermediaEngine.Responses;

namespace HypermediaEngine.Abstractions;

public interface IObjectMetadataHandler<T>
    where T : notnull
{
    IHypermediaObjectBuilder<T> Handle(T? result, ObjectResponseMetadata? metadata = null);
}
public interface ICollectionMetadataHandler<T>
    where T : notnull
{
    IHypermediaCollectionBuilder<T> Handle(IEnumerable<T> result, ListResponseMetadata? metadata = null);
}
