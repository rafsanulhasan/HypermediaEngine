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

    public static readonly FilterOperator Eq = new(EqKey, 1);
    public static readonly FilterOperator Ne = new(NeKey, 2);
    public static readonly FilterOperator Gt = new(GtKey, 4);
    public static readonly FilterOperator Gte = new(GteKey, 8);
    public static readonly FilterOperator Lt = new(LtKey, 16);
    public static readonly FilterOperator Lte = new(LteKey, 32);
    public static readonly FilterOperator In = new(InKey, 64);
    public static readonly FilterOperator NotIn = new(NotInKey, 128);
    public static readonly FilterOperator Contains = new(ContainsKey, 256);
    public static readonly FilterOperator StartsWith = new(StartsWithKey, 512);
    public static readonly FilterOperator EndsWith = new(EndsWithKey, 1024);

    private FilterOperator(string name, int value)
        : base(name, value) { }

    public static implicit operator string(FilterOperator filterOperator) => filterOperator.Name;
    public static implicit operator FilterOperator?(string filterOperator) 
        => string.IsNullOrWhiteSpace(filterOperator)
         ? null
         : List.FirstOrDefault(l => l.Name.Equals(filterOperator, StringComparison.Ordinal));
}
