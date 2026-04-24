using Ardalis.SmartEnum;

namespace HypermediaEngine.Responses.Handlers;

public sealed class LinkHandlers : SmartEnum<LinkHandlers, int>
{
    public const string ObjectLinkHandlersKey = nameof(ObjectLinkHandlers);
    public const string CollectionLinkHandlersKey = nameof(CollectionLinkHandlers);

    public static readonly LinkHandlers ObjectLinkHandlers = new(nameof(ObjectLinkHandlersKey), 1);
    public static readonly LinkHandlers CollectionLinkHandlers = new(nameof(CollectionLinkHandlersKey), 2);

    private LinkHandlers(string name, int value) 
        : base(name, value)
    {
    }
}
