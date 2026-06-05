using LanguageExt.Common;

using Microsoft.Extensions.Logging;

using SynergyFx.EntityTagCaching.Models;
using SynergyFx.HypermediaEngine.Abstractions;

namespace SynergyFx.HypermediaEngine.Responses.Handlers;

internal class ObjectEtagMetadataHandler<T>(ILogger<ObjectEtagMetadataHandler<T>> logger)
    : AbstractObjectMetadataHandler<T> where T : notnull
{
    public override IHypermediaObjectBuilder<T> Handle(T? result, ObjectResponseMetadata? metadata = null)
    {
        metadata ??= new ObjectResponseMetadata(EntityTag.Empty);
        OptionalResult<EntityTag> etagResult = EntityTag.Create(result);

        metadata = etagResult.Match(
            entityTag => metadata with
            {
                EntityTag = entityTag,
            },
            () =>
            {
                logger.LogWarning("Failed to create ETag for object of type {ObjectType}", typeof(T).Name);
                return metadata;
            },
            ex =>
            {
                logger.LogWarning(ex, "Failed to create ETag for object of type {ObjectType}", typeof(T).Name);
                return metadata;
            });

        return Builder!.WithMetadata(metadata);
    }
}
