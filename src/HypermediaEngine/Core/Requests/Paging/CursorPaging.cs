namespace SynergyFx.HypermediaEngine.Requests.Paging;

public sealed record CursorPaging
{
    public CursorPaging(int limit, string? cursor, string? field)
    {
        Cursor = cursor;
        Limit = limit;
        Field = field;
    }

    internal CursorPaging() { }

    public string? Cursor { get; init; }
    public int Limit { get; init; }
    public string? Field { get; init; }
}
