using Ardalis.SmartEnum;

namespace HypermediaEngine.Responses.Rules;

public sealed class CollectonPipelineRules
    : SmartEnum<CollectonPipelineRules, int>
{
    public const string CollectionRuleName = nameof(Collection);
    public const string QueryableRuleName = nameof(Queryable);
    public const string MartenQueryableRuleName = nameof(MartenQueryable);

    public static readonly CollectonPipelineRules Collection = new(CollectionRuleName, 1 << 0);
    public static readonly CollectonPipelineRules Queryable = new(QueryableRuleName, 1 << 1);
    public static readonly CollectonPipelineRules MartenQueryable = new(MartenQueryableRuleName, 1 << 2);

    private CollectonPipelineRules(string name, int value)
        : base(name, value) { }
}
