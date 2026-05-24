using Ardalis.SmartEnum.SystemTextJson;

using System.Text.Json.Serialization;

namespace SynergyFx.HypermediaEngine.Requests.Sorting;

public sealed record class SortField
{
    public SortField(string field, SortDirection direction)
    {
        Field = field;
        Direction = direction;
    }

    [JsonConstructor]
    internal SortField() { }

    public string Field { get; set; }
    [JsonConverter(typeof(SmartEnumNameConverter<SortDirection, int>))]
    public SortDirection Direction { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Field} {Direction}";
    }
}
