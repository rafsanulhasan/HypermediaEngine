using Microsoft.AspNetCore.Mvc;

namespace HypermediaEngine.Requests.Paging;

public sealed record CursorPaging
{
    public CursorPaging(string? cursor, int limit)
    {
        Cursor = cursor;
        Limit = limit;
    }

    internal CursorPaging() { }

    [FromQuery]
    public string? Cursor { get; init; }
    [FromQuery]
    public int Limit { get; init; }
}
