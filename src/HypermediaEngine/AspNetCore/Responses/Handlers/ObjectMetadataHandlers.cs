using Ardalis.SmartEnum;

namespace HypermediaEngine.Responses.Handlers;

public sealed class ObjectMetadataHandlers : SmartEnum<ObjectMetadataHandlers, int>
{
    public static readonly ObjectMetadataHandlers ApiVersion = new(nameof(ObjectApiVersionMetadataHandler<>), 1);
    public static readonly ObjectMetadataHandlers EntityVersion = new(nameof(ObjectEntityVersionMetadataHandler<>), 2);
    public static readonly ObjectMetadataHandlers ETag = new(nameof(ObjectEtagMetadataHandler<>), 3);

    private ObjectMetadataHandlers(string name, int value)
        : base(name, value)
    {
    }

    public static implicit operator string(ObjectMetadataHandlers handler)
    {
        return handler.Name;
    }

    public static implicit operator int(ObjectMetadataHandlers handler)
    {
        return handler.Value;
    }
}
