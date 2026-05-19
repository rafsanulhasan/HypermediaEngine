using Ardalis.SmartEnum;

namespace HypermediaEngine.Responses.Handlers;

public sealed class CollectionResponseHandlers : SmartEnum<CollectionResponseHandlers, int>
{
    public const string EnumerableKey = nameof(Enumerable);
    public const string QueryableKey = nameof(Queryable);
    public const string MartenQueryableKey = nameof(MartenQueryable);
    public static readonly CollectionResponseHandlers Enumerable = new(EnumerableKey, 1 << 1);
    public static readonly CollectionResponseHandlers Queryable = new(QueryableKey, 1 << 2);
    public static readonly CollectionResponseHandlers MartenQueryable = new(MartenQueryableKey, 1 << 3);
    private CollectionResponseHandlers(string name, int value)
        : base(name, value)
    {
    }
}