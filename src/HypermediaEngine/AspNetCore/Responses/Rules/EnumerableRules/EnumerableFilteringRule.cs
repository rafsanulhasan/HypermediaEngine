using LanguageExt;

using SynergyFx.HypermediaEngine.Abstractions;
using SynergyFx.HypermediaEngine.Requests.Filtering;
using SynergyFx.HypermediaEngine.Responses.Metadata;

namespace SynergyFx.HypermediaEngine.Responses.Rules.EnumerableRules;

internal sealed class EnumerableFilteringRule<T>(
    ICollectionResponsePipeline<T> next,
    IFilterNodeHandler filterNodeHandler
) : ICollectionResponsePipeline<T>
    where T : notnull
{
    public async ValueTask<CollectionResponseContext<T>> Process(CollectionResponseContext<T> context)
    {
        IEnumerable<T> filteredItems = context.Filter.Match(filter =>
                                       {
                                           string filterString = filterNodeHandler.Handle(filter);
                                           return string.IsNullOrWhiteSpace(filterString)
                                                ? context.Items
                                                : context.Items.WhereDynamic($"x => {filterString}");
                                       },
                                       () => context.Items);
        int filteredItemCount = filteredItems.Count();
        if (filteredItemCount == 0)
        {
            return context with
            {
                FilteredItems = Option<IEnumerable<T>>.Some([]),
                PagingMetadata = context.PagingMetadata.Match(
                    Some: paging => paging with
                    {
                        TotalCount = 0,
                    },
                    None: () => new PagingMetadata()
                    {
                        TotalCount = 0,
                    }),
            };
        }
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
