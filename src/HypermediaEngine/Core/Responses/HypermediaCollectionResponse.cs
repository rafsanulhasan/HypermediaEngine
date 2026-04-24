namespace HypermediaEngine.Responses;

/// <summary>
/// Represents a hypermedia collection response, containing a list of items and related metadata.
/// </summary>
/// <typeparam name="T">The type of the items in the collection.</typeparam>
public sealed record class HypermediaCollectionResponse<T> 
    where T : notnull
{
    /// <summary>
    /// Initializes a new instance of the HypermediaCollectionResponse class with the specified collection of items and
    /// the total number of items available.
    /// </summary>
    /// <remarks>This constructor is typically used to create paginated responses in hypermedia APIs, allowing
    /// clients to understand both the current subset of items and the total number of items available for
    /// pagination.</remarks>
    /// <param name="stream">The collection of items to include in the response. This parameter cannot be null.</param>
    /// <param name="count">The total number of items available in the full collection, which may be greater than the number of items in the
    /// current page. Must be a non-negative integer.</param>
    public HypermediaCollectionResponse(IEnumerable<T> stream, int count)
    {
        Items = stream;
        TotalCount = count;
    }

    /// <summary>
    /// Initializes a new instance of the HypermediaCollectionResponse class that contains the specified collection of
    /// items.
    /// </summary>
    /// <remarks>The TotalCount property is automatically set to the number of items in the provided
    /// list.</remarks>
    /// <param name="items">A list of items of type T to include in the response. Cannot be null.</param>
    public HypermediaCollectionResponse(IList<T> items)
    {
        Items = items;
        TotalCount = items.Count;
    }

    /// <summary>
    /// Initializes a new instance of the HypermediaCollectionResponse class that contains the specified collection of
    /// items.
    /// </summary>
    /// <remarks>The TotalCount property is automatically set to the number of items in the provided
    /// list.</remarks>
    /// <param name="items">A collection of items of type T to include in the response. Cannot be null.</param>
    public HypermediaCollectionResponse(ICollection<T> items)
    {
        Items = items;
        TotalCount = items.Count;
    }

    /// <summary>
    /// Initializes a new instance of the HypermediaCollectionResponse class that contains the specified collection of
    /// items.
    /// </summary>
    /// <remarks>The TotalCount property is automatically set to the number of items in the provided
    /// collection.</remarks>
    /// <param name="items">A collection of items of type T to include in the response. This parameter cannot be null.</param>
    public HypermediaCollectionResponse(HashSet<T> items)
    {
        Items = items;
        TotalCount = items.Count;
    }

    /// <summary>
    /// Initializes a new instance of the HypermediaCollectionResponse class with the specified array of items.
    /// </summary>
    /// <remarks>The TotalCount property is automatically set to the number of elements in the provided items
    /// array.</remarks>
    /// <param name="items">An array of items to include in the collection response. This parameter cannot be null.</param>
    public HypermediaCollectionResponse(T[] items)
    {
        Items = items;
        TotalCount = items.Length;
    }

    /// <summary>
    /// Gets the collection of items contained in the response.
    /// </summary>
    /// <remarks>This property is initialized with an empty collection. The collection is read-only and cannot
    /// be modified after initialization.</remarks>
    public IEnumerable<T> Items { get; set; } = [];

    /// <summary>
    /// Gets or sets the total number of items in the collection.
    /// </summary>
    /// <remarks>This property reflects the current count of items and can be used to determine the size of
    /// the collection. It is updated automatically as items are added or removed.</remarks>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets a collection of hypermedia links associated with the resource, where each key represents a link
    /// relation type and the value is the corresponding hypermedia link.
    /// </summary>
    /// <remarks>Use this property to provide clients with navigational or actionable links in a
    /// hypermedia-driven API. Each entry enables clients to discover related resources or available operations
    /// dynamically.</remarks>
    public ListLinkCollection Links { get; set; } = new();

    /// <summary>
    /// Gets or sets the metadata associated with the hypermedia resource.
    /// </summary>
    /// <remarks>This property may be null if no metadata is available. Use this property to access additional
    /// information or context about the resource, such as descriptive details or processing hints.</remarks>
    public ListResponseMetadata? Meta { get; set; }
}
