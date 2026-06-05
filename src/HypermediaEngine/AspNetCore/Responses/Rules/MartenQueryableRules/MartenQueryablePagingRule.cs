using LanguageExt;

using SynergyFx.HypermediaEngine.Abstractions;
using SynergyFx.HypermediaEngine.Helpers;
using SynergyFx.HypermediaEngine.Requests.Paging;

namespace SynergyFx.HypermediaEngine.Responses.Rules.MartenQueryableRules;

internal sealed class MartenQueryablePagingRule<T>(ICollectionResponsePipeline<T> next)
    : ICollectionResponsePipeline<T>
    where T : notnull
{
    public async ValueTask<CollectionResponseContext<T>> Process(CollectionResponseContext<T> context)
    {
        IQueryable<T> source = context.FilteredQuery.Match(
            Some: q => q,
            None: () => context.Query.Match(
                Some: q => q,
                None: () => throw new InvalidOperationException(
                    "CollectionResponseContext must have FilteredQuery or Query set before the MartenQueryable paging rule.")));

        OffsetOrCursorPaging offsetOrCursorPaging = context.QueryParams.Match(
            Some: queryParams => OffsetOrCursorPaging.GetOrDefault(queryParams.Paging),
            None: () => OffsetOrCursorPaging.Default);

        IQueryable<T> pagedQuery = source.ApplyPaging(offsetOrCursorPaging);

        context = context with
        {
            PagedQuery = Option<IQueryable<T>>.Some(pagedQuery),
        };

        return await next.Process(context).ConfigureAwait(false);
    }
}
