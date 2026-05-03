using HypermediaEngine.Requests;
using HypermediaEngine.Requests.Filtering;
using HypermediaEngine.Responses.Metadata;

using LanguageExt;

namespace HypermediaEngine.Responses;

public sealed record class CollectionResponseContext<T>
    where T : notnull
{
    public CollectionResponseContext(IEnumerable<T> collection)
    {
        Items = collection;
    }

    public CollectionResponseContext(IQueryable<T> query)
    {
        Query = Option<IQueryable<T>>.Some(query);
    }

    public Option<QueryParams> QueryParams { get; init; } = Option<QueryParams>.None;
    public Option<FilterNode> Filter { get; init; } = Option<FilterNode>.None;
    public Option<PagingMetadata> PagingMetadata { get; init; } = Option<PagingMetadata>.None;
    public Option<IReadOnlyList<SortingMetadata>> SortingMetadata { get; init; } = Option<IReadOnlyList<SortingMetadata>>.None;

    public IEnumerable<T> Items { get; init; }
    public Option<IEnumerable<T>> FilteredItems { get; init; } = Option<IEnumerable<T>>.None;
    public Option<IEnumerable<T>> PagedItems { get; init; } = Option<IEnumerable<T>>.None;
    public Option<IEnumerable<T>> SortedItems { get; init; } = Option<IEnumerable<T>>.None;
    public Option<IReadOnlyList<T>> FinalItems { get; init; } = Option<IReadOnlyList<T>>.None;

    public Option<IQueryable<T>> Query { get; init; } = Option<IQueryable<T>>.None;
    public Option<IQueryable<T>> FilteredQuery { get; init; } = Option<IQueryable<T>>.None;
    public Option<IQueryable<T>> PagedQuery { get; init; } = Option<IQueryable<T>>.None;
    public Option<IQueryable<T>> SortedQuery { get; init; } = Option<IQueryable<T>>.None;
}