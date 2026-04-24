using Ardalis.SmartEnum;

namespace HypermediaEngine.Requests.Sorting;

public sealed class SortDirection : SmartEnum<SortDirection, int>
{
    public static readonly SortDirection Ascending = new(nameof(Ascending), 1);
    public static readonly SortDirection Descending = new(nameof(Descending), 2);

    internal SortDirection() : base(string.Empty, -1)
    {

    }

    private SortDirection(string name, int value)
        : base(name, value)
    {
    }

    public static implicit operator string(SortDirection sortDirection) => sortDirection.Name;
}
