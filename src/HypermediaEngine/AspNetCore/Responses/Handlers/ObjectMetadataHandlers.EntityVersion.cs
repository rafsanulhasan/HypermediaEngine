using EntityTagCaching.Models;

using HypermediaEngine.Abstractions;

namespace HypermediaEngine.Responses.Handlers;

internal class ObjectEntityVersionMetadataHandler<T> 
    : AbstractObjectMetadataHandler<T> where T : notnull
{
    public override IHypermediaObjectBuilder<T> Handle(T? result, ObjectResponseMetadata? metadata = null)
    {
        metadata ??= new ObjectResponseMetadata(EntityTag.Empty);
        if (result is IVersionableEntity versionable)
        {
            metadata = metadata with
            {
                EntityVersion = versionable.Version,
            };
        }
        return Builder!.WithMetadata(metadata);
    }
}
