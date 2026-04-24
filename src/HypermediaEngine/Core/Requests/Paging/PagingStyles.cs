using Ardalis.SmartEnum;

namespace HypermediaEngine.Requests.Paging;

public sealed class PagingStyles : SmartEnum<PagingStyles, int>
{
    public const string OffsetKey = nameof(Offset);
    public const string CursorKey = nameof(Cursor);

    public static readonly PagingStyles Offset = new(OffsetKey, 1);
    public static readonly PagingStyles Cursor = new(CursorKey, 2);

    public static implicit operator string(PagingStyles style)
    {
        return style.Name;
    }

    internal PagingStyles() : base(string.Empty, -1) { }

    private PagingStyles(string name, int value)
        : base(name, value)
    {
    }
}
