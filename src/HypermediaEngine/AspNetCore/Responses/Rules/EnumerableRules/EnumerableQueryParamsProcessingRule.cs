using LanguageExt;

using SynergyFx.HypermediaEngine.Abstractions;
using SynergyFx.HypermediaEngine.Requests.Filtering;

namespace SynergyFx.HypermediaEngine.Responses.Rules.EnumerableRules;

internal sealed class EnumerableQueryParamsProcessingRule<T>(ICollectionResponsePipeline<T> next)
    : ICollectionResponsePipeline<T>
    where T : notnull
{
    public async ValueTask<CollectionResponseContext<T>> Process(CollectionResponseContext<T> context)
    {
        context = context with
        {
            Filter = context.QueryParams.Match(
                q => q.Body?.Filtering is FilterNode filterNode
                   ? filterNode
                   : Option<FilterNode>.None,
                () => Option<FilterNode>.None),
        };

        return await next.Process(context).ConfigureAwait(false);
    }
}
