using Ardalis.SmartEnum;

namespace HypermediaEngine.Responses.Handlers;

public sealed class ResponseHandlers : SmartEnum<ResponseHandlers, int>
{
    public const string ObjectHandlersKey = nameof(ObjectHandlers);
    public const string CollectionHandlersKey = nameof(CollectionHandlers);

    public static readonly ResponseHandlers ObjectHandlers = new(ObjectHandlersKey, 1);
    public static readonly ResponseHandlers CollectionHandlers = new(CollectionHandlersKey, 2);

    private ResponseHandlers(string name, int value)
        : base(name, value)
    {
    }
}


