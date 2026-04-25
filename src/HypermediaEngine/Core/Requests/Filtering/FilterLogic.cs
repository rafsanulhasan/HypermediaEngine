using Ardalis.SmartEnum;
using Ardalis.SmartEnum.SystemTextJson;

using System.Text.Json.Serialization;

namespace HypermediaEngine.Requests.Filtering;

[JsonConverter(typeof(SmartEnumNameConverter<FilterLogic, int>))]
public sealed class FilterLogic : SmartEnum<FilterLogic, int>
{
    public static readonly FilterLogic And = new(nameof(And), 1 << 1);
    public static readonly FilterLogic Or = new(nameof(Or), 1 << 2);

    private FilterLogic(string name, int value)
        : base(name, value) { }
}
