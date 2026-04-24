using Microsoft.AspNetCore.Mvc;

namespace HypermediaEngine.Requests.Paging;

public sealed record OffsetPaging
{
    public OffsetPaging(int page, int pageSize)
    {
        Page = page;
        PageSize = pageSize;
    }

    internal OffsetPaging() { }

    [FromQuery]
    public int Page { get; set; }
    [FromQuery]
    public int PageSize { get; set; }
}
