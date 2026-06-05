using Ardalis.SmartEnum;
using Ardalis.SmartEnum.SystemTextJson;

using System.Text.Json.Serialization;

namespace SynergyFx.HypermediaEngine.Requests.Filtering;

[JsonConverter(typeof(SmartEnumNameConverter<FilterLogic, int>))]
public sealed class FilterLogic : SmartEnum<FilterLogic, int>
{
    public const string AndKey = nameof(And);
    public const string OrKey = nameof(Or);

    public static readonly FilterLogic And = new(AndKey, 1 << 1, "&&");
    public static readonly FilterLogic Or = new(OrKey, 1 << 2, "||");

    private FilterLogic(string name, int value, string @operator)
        : base(name, value)
    {
        Operator = @operator;
    }

    public string Operator { get; private set; }

    public static implicit operator string(FilterLogic filterLogic) => filterLogic.Name;
    public static implicit operator FilterLogic(string filterLogic) => List.Single(fl => fl.Name == filterLogic);
}
