using HypermediaEngine.Abstractions;
using HypermediaEngine.Responses.Metadata;

using LanguageExt;

namespace HypermediaEngine.Responses.Rules.EnumerableRules;

internal sealed class EnumerableFilteringRule<T>(ICollectionResponsePipeline<T> next)
    : ICollectionResponsePipeline<T>
    where T : notnull
{
    public async ValueTask<CollectionResponseContext<T>> Process(CollectionResponseContext<T> context)
    {
        IEnumerable<T> filteredItems = context.Filter.Match(
                                        filter =>
                                        {
                                            string filterString = filter.ToString();
                                            return string.IsNullOrWhiteSpace(filterString)
                                                    ? context.Items
                                                    : context.Items.WhereDynamic($"x => x.{filter}");
                                        },
                                        () => context.Items);
        int filteredItemCount = filteredItems.Count();
        PagingMetadata pagingMetadata = context.PagingMetadata.Match(
            Some: paging => paging with
            {
                TotalCount = filteredItemCount,
            },
            None: () => new PagingMetadata()
            {
                TotalCount = filteredItemCount,
            });

        context = context with
        {
            FilteredItems = Option<IEnumerable<T>>.Some(filteredItems),
            PagingMetadata = pagingMetadata,
        };

        return await next.Process(context).ConfigureAwait(false);
    }
}
