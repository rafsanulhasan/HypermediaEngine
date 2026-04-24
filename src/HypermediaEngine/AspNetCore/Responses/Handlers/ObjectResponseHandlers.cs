using Ardalis.SmartEnum;

namespace HypermediaEngine.AspNetCore.Responses.Handlers;

public sealed class ObjectResponseHandlers : SmartEnum<ObjectResponseHandlers, int>
{
    public const string TResponseHandlerKey = nameof(TResponseHandler);
    public const string OkOfTResponseHandlerKey = nameof(OkOfTResponseHandler);
    public const string JsonHttpResultOfTResponseHandlerKey = nameof(JsonHttpResultOfTResponseHandler);
    public const string ObjectResponseHandlerKey = nameof(ObjectResponseHandler);
        
    public static readonly ObjectResponseHandlers TResponseHandler = new(TResponseHandlerKey, 1);
    public static readonly ObjectResponseHandlers OkOfTResponseHandler = new(OkOfTResponseHandlerKey, 2);
    public static readonly ObjectResponseHandlers JsonHttpResultOfTResponseHandler = new(JsonHttpResultOfTResponseHandlerKey, 3);
    public static readonly ObjectResponseHandlers ObjectResponseHandler = new(ObjectResponseHandlerKey, 4);

    private ObjectResponseHandlers(string name, int value) 
        : base(name, value)
    {
    }
}