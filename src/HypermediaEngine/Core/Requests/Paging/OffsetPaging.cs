namespace HypermediaEngine.Requests.Paging;

public sealed record class OffsetPaging
{
    public static readonly OffsetPaging Default = new(page: 1, pageSize: 10);

    public OffsetPaging(int page, int pageSize)
    {
        Page = page;
        PageSize = pageSize;
    }

    internal OffsetPaging() { }

    public int Page { get; set; }
    public int PageSize { get; set; }
}
