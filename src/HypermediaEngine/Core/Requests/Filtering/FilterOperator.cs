using Ardalis.SmartEnum;
using Ardalis.SmartEnum.SystemTextJson;

using System.Text.Json.Serialization;

namespace HypermediaEngine.Requests.Filtering;

[JsonConverter(typeof(SmartEnumNameConverter<FilterOperator, int>))]
public sealed class FilterOperator : SmartEnum<FilterOperator, int>
{
    public const string EqKey = "eq";
    public const string NeKey = "ne";
    public const string GtKey = "gt";
    public const string GteKey = "gte";
    public const string LtKey = "lt";
    public const string LteKey = "lte";
    public const string InKey = "in";
    public const string NotInKey = "not in";
    public const string ContainsKey = "contains";
    public const string StartsWithKey = "starts with";
    public const string EndsWithKey = "ends with";

    public static readonly FilterOperator Eq = new(EqKey, 1 << 1);
    public static readonly FilterOperator Ne = new(NeKey, 1 << 2);
    public static readonly FilterOperator Gt = new(GtKey, 1 << 4);
    public static readonly FilterOperator Gte = new(GteKey, 1 << 8);
    public static readonly FilterOperator Lt = new(LtKey, 1 << 16);
    public static readonly FilterOperator Lte = new(LteKey, 1 << 32);
    public static readonly FilterOperator In = new(InKey, 1 << 64);
    public static readonly FilterOperator NotIn = new(NotInKey, 1 << 128);
    public static readonly FilterOperator Contains = new(ContainsKey, 1 << 256);
    public static readonly FilterOperator StartsWith = new(StartsWithKey, 1 << 512);
    public static readonly FilterOperator EndsWith = new(EndsWithKey, 1 << 1024);

    private FilterOperator(string name, int value)
        : base(name, value) { }

    public static implicit operator string(FilterOperator filterOperator) => filterOperator.Name;
}
