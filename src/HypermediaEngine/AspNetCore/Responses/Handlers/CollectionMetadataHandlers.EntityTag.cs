using EntityTagCaching.Models;

using HypermediaEngine.Abstractions;

namespace HypermediaEngine.Responses.Handlers;

#pragma warning disable MA0048 // File name must match type name
internal sealed class CollectionETagMetadataHandler<T> : AbstractCollectionMetadataHandler<T>
    where T : notnull
{
    public override IHypermediaCollectionBuilder<T> Handle(IEnumerable<T> result, ListResponseMetadata? metadata = null)
    {
        metadata ??= Builder?.Metadata;
        EntityTag entityTag = EntityTag.Create(result).Match(
            entityTag => entityTag,
            () => EntityTag.Empty,
            _ => EntityTag.Empty
        );
        metadata = metadata is null
                 ? new ListResponseMetadata(entityTag)
                 : metadata with
                 {
                     EntityTag = entityTag,
                 };  
        return Builder!.WithMetadata(metadata);
    }
}
#pragma warning restore MA0048 // File name must match type name
