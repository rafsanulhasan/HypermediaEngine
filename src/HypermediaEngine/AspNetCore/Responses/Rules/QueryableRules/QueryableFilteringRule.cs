using LanguageExt;

using SynergyFx.HypermediaEngine.Abstractions;
using SynergyFx.HypermediaEngine.Responses.Metadata;

namespace SynergyFx.HypermediaEngine.Responses.Rules.QueryableRules;

internal sealed class QueryableFilteringRule<T>(ICollectionResponsePipeline<T> next)
    : ICollectionResponsePipeline<T>
    where T : notnull
{
    public async ValueTask<CollectionResponseContext<T>> Process(CollectionResponseContext<T> context)
    {
        IQueryable<T> filteredQuery = context.Query.Match(
            Some: query => context.Filter.Match(
                filter => query.WhereDynamic(filter.ToString()),
                () => query),
            None: () => Enumerable.Empty<T>().AsQueryable());
        int totalCount = filteredQuery.Count();
        PagingMetadata pagingMetadata = context.PagingMetadata.Match(
            Some: paging => paging with
            {
                TotalCount = totalCount,
            },
            None: () => new PagingMetadata()
            {
                TotalCount = totalCount,
            });

        context = context with
        {
            FilteredQuery = Option<IQueryable<T>>.Some(filteredQuery),
            PagingMetadata = pagingMetadata,
        };

        return await next.Process(context).ConfigureAwait(false);
    }
}
