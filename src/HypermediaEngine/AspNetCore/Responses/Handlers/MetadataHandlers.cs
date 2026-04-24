using Ardalis.SmartEnum;

namespace HypermediaEngine.Responses.Handlers;

public sealed class MetadataHandlers : SmartEnum<MetadataHandlers, int>
{
    public const string ObjectMetadataHandlersKey = nameof(ObjectMetadataHandlers);
    public const string CollectionMetadataHandlersKey = nameof(CollectionMetadataHandlers);

    public static readonly MetadataHandlers ObjectMetadataHandlers = new(nameof(ObjectMetadataHandlersKey), 1);
    public static readonly MetadataHandlers CollectionMetadataHandlers = new(nameof(CollectionMetadataHandlersKey), 2);

    private MetadataHandlers(string name, int value) 
        : base(name, value)
    {
    }
}


