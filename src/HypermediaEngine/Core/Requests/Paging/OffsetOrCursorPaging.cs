using LanguageExt;

using OneOf;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace HypermediaEngine.Requests.Paging;

public sealed class OffsetOrCursorPaging
    : OneOfBase<OffsetPaging, CursorPaging>
{
    public OffsetOrCursorPaging(OneOf<OffsetPaging, CursorPaging> input)
        : base(input)
    {
        _input = input;
    }
    public OffsetOrCursorPaging()
        : base(new OffsetPaging(1, 10))
    {
        _input = new OffsetPaging(1, 10);
    }

    private readonly OneOf<OffsetPaging, CursorPaging>? _input;
    public int PageSize => Match(op => op.PageSize, cp => cp.Limit);
    public Option<int> CurrentPage => Match(op => op.Page, _ => Option<int>.None);

    public PagingStyles Style => Match(_ => PagingStyles.Offset, _ => PagingStyles.Cursor);
    public Option<string> Cursor => Match(
        _ => Option<string>.None,
        cp => string.IsNullOrWhiteSpace(cp.Cursor)
            ? Option<string>.None
            : cp.Cursor);

    public static implicit operator OneOf<OffsetPaging, CursorPaging>?(OffsetOrCursorPaging ocp)
    {
        return ocp._input;
    }

    public static implicit operator OffsetOrCursorPaging(OffsetPaging op)
    {
        return new OffsetOrCursorPaging(op);
    }
    public static implicit operator OffsetOrCursorPaging(CursorPaging cp)
    {
        return new OffsetOrCursorPaging(cp);
    }

    public bool IsOffset(out OffsetPaging offsetPaging) => TryPickT0(out offsetPaging, out _);
    public bool IsCursor(out CursorPaging cursorPaging) => TryPickT1(out cursorPaging, out _);

    public static bool TryParse(string input, [NotNull] out OffsetOrCursorPaging result)
    {
        input = input.Trim().Replace("?", string.Empty, StringComparison.OrdinalIgnoreCase);
        Dictionary<string, string> query = input
            .Split(
                '&', 
                StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(part => part.Split('='))
            .Where(parts => parts.Length == 2)
            .ToDictionary(parts => parts[0].Trim(), parts => parts[1].Trim(), StringComparer.OrdinalIgnoreCase);
        if (query.TryGetValue("page", out string? pageValue)
         && query.TryGetValue("pageSize", out string? pageSizeValue)
         && int.TryParse(pageValue, CultureInfo.InvariantCulture, out int page)
         && int.TryParse(pageSizeValue, CultureInfo.InvariantCulture, out int pageSize))
        {
            result = new OffsetPaging(page, pageSize);
            return true;
        }

        if (query.TryGetValue("limit", out string? limitValue)
         && query.TryGetValue("cursor", out string? cursor)
         && int.TryParse(limitValue, CultureInfo.InvariantCulture, out int limit))
        {
            result = new CursorPaging(cursor, limit);
            return true;
        }
        result = new OffsetPaging(1, 10);
        return true;
    }
}
