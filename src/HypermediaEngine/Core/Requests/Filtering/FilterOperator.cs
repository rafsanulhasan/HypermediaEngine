using Ardalis.SmartEnum;
using Ardalis.SmartEnum.SystemTextJson;

using System.Text.Json.Serialization;

namespace SynergyFx.HypermediaEngine.Requests.Filtering;

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
    public const string NotContainsKey = "not contains";
    public const string StartsWithKey = "starts with";
    public const string NotStartsWithKey = "not starts with";
    public const string EndsWithKey = "ends with";
    public const string NotEndsWithKey = "not ends with";

    public static readonly FilterOperator Eq = new(EqKey, 1, [typeof(string), typeof(int), typeof(uint), typeof(short), typeof(ushort), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal), typeof(DateTime), typeof(DateTimeOffset), typeof(bool), typeof(Guid)]);
    public static readonly FilterOperator Ne = new(NeKey, 2, [typeof(string), typeof(int), typeof(uint), typeof(short), typeof(ushort), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal), typeof(DateTime), typeof(DateTimeOffset), typeof(bool), typeof(Guid)]);
    public static readonly FilterOperator Gt = new(GtKey, 4, [typeof(int), typeof(uint), typeof(short), typeof(ushort), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal), typeof(DateTime), typeof(DateTimeOffset)]);
    public static readonly FilterOperator Gte = new(GteKey, 8, [typeof(int), typeof(uint), typeof(short), typeof(ushort), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal), typeof(DateTime), typeof(DateTimeOffset)]);
    public static readonly FilterOperator Lt = new(LtKey, 16, [typeof(int), typeof(uint), typeof(short), typeof(ushort), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal), typeof(DateTime), typeof(DateTimeOffset)]);
    public static readonly FilterOperator Lte = new(LteKey, 32, [typeof(int), typeof(uint), typeof(short), typeof(ushort), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal), typeof(DateTime), typeof(DateTimeOffset)]);
    public static readonly FilterOperator In = new(InKey, 64, [typeof(IEnumerable<string>), typeof(IEnumerable<int>), typeof(IEnumerable<uint>), typeof(IEnumerable<short>), typeof(IEnumerable<ushort>), typeof(IEnumerable<long>), typeof(IEnumerable<ulong>), typeof(IEnumerable<float>), typeof(IEnumerable<double>), typeof(IEnumerable<decimal>), typeof(IEnumerable<DateTime>), typeof(IEnumerable<DateTimeOffset>)]);
    public static readonly FilterOperator NotIn = new(NotInKey, 128, [typeof(IEnumerable<string>), typeof(IEnumerable<int>), typeof(IEnumerable<uint>), typeof(IEnumerable<short>), typeof(IEnumerable<ushort>), typeof(IEnumerable<long>), typeof(IEnumerable<ulong>), typeof(IEnumerable<float>), typeof(IEnumerable<double>), typeof(IEnumerable<decimal>), typeof(IEnumerable<DateTime>), typeof(IEnumerable<DateTimeOffset>)]);
    public static readonly FilterOperator Contains = new(ContainsKey, 256, [typeof(string)]);
    public static readonly FilterOperator NotContains = new(NotContainsKey, 512, [typeof(string)]);
    public static readonly FilterOperator StartsWith = new(StartsWithKey, 1024, [typeof(string)]);
    public static readonly FilterOperator NotStartsWith = new(NotStartsWithKey, 2048, [typeof(string)]);
    public static readonly FilterOperator EndsWith = new(EndsWithKey, 4096, [typeof(string)]);
    public static readonly FilterOperator NotEndsWith = new(NotEndsWithKey, 8192, [typeof(string)]);

    private FilterOperator(string name, int value, Type[] supportedTypes)
        : base(name, value)
    {
        SupportedTypes = supportedTypes;
    }

    [JsonIgnore]
    public IReadOnlyList<Type> SupportedTypes { get; private set; }

    public static implicit operator string(FilterOperator filterOperator) => filterOperator.Name;
    public static implicit operator FilterOperator?(string filterOperator)
        => string.IsNullOrWhiteSpace(filterOperator)
         ? null
         : List.FirstOrDefault(l => l.Name.Equals(filterOperator, StringComparison.Ordinal));

    public bool IsValidForType(Type type)
    {
        return SupportedTypes.Any(t => t.IsAssignableFrom(type) || type.IsAssignableFrom(t));
    }

    public bool IsValidForTypes(params IEnumerable<Type> types)
    {
        return SupportedTypes.Any(t => types.Any(type => t.IsAssignableFrom(type) || type.IsAssignableFrom(t)));
    }
}
