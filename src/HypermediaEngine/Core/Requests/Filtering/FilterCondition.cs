using System.Text.Json;

namespace HypermediaEngine.Requests.Filtering;

public sealed record class FilterCondition(
    string Field,
    FilterOperator Operator,
    object? Value
)
{
    /// <inheritdoc />
    public override string ToString()
    {
        object? val = Value.ExtractStrongType();
        string cond = (val, Operator.Name) switch
        {
            (null, _) => string.Empty,
            (IEnumerable<Guid> list, FilterOperator.InKey) =>
                $"{Field} in ({string.Join(", ", list.Select(i => $"\"{i}\""))})",
            (IEnumerable<Guid> list, FilterOperator.NotInKey) =>
                $"{Field} not in ({string.Join(", ", list.Select(i => $"\"{i}\""))})",
            (IEnumerable<string> list, FilterOperator.InKey) =>
                $"{Field} in ({string.Join(", ", list.Select(i => $"\"{i}\""))})",
            (IEnumerable<string> list, FilterOperator.NotInKey) =>
                $"{Field} not in ({string.Join(", ", list.Select(i => $"\"{i}\""))})",
            (IEnumerable<int> list, FilterOperator.InKey) =>
                $"{Field} in ({string.Join(", ", list)})",
            (IEnumerable<int> list, FilterOperator.NotInKey) =>
                $"{Field} not in ({string.Join(", ", list)})",
            (IEnumerable<uint> list, FilterOperator.InKey) =>
                $"{Field} in ({string.Join(", ", list)})",
            (IEnumerable<uint> list, FilterOperator.NotInKey) =>
                $"{Field} not in ({string.Join(", ", list)})",
            (IEnumerable<short> list, FilterOperator.InKey) =>
                $"{Field} in ({string.Join(", ", list)})",
            (IEnumerable<short> list, FilterOperator.NotInKey) =>
                $"{Field} not in ({string.Join(", ", list)})",
            (IEnumerable<ushort> list, FilterOperator.InKey) =>
                $"{Field} in ({string.Join(", ", list)})",
            (IEnumerable<ushort> list, FilterOperator.NotInKey) =>
                $"{Field} not in ({string.Join(", ", list)})",
            (IEnumerable<long> list, FilterOperator.InKey) =>
                $"{Field} in ({string.Join(", ", list)})",
            (IEnumerable<long> list, FilterOperator.NotInKey) =>
                $"{Field} not in ({string.Join(", ", list)})",
            (IEnumerable<ulong> list, FilterOperator.InKey) =>
                $"{Field} in ({string.Join(", ", list)})",
            (IEnumerable<ulong> list, FilterOperator.NotInKey) =>
                $"{Field} not in ({string.Join(", ", list)})",
            (IEnumerable<float> list, FilterOperator.InKey) =>
                $"{Field} in ({string.Join(", ", list)})",
            (IEnumerable<float> list, FilterOperator.NotInKey) =>
                $"{Field} not in ({string.Join(", ", list)})",
            (IEnumerable<double> list, FilterOperator.InKey) =>
                $"{Field} in ({string.Join(", ", list)})",
            (IEnumerable<double> list, FilterOperator.NotInKey) =>
                $"{Field} not in ({string.Join(", ", list)})",
            (IEnumerable<decimal> list, FilterOperator.InKey) =>
                $"{Field} in ({string.Join(", ", list)})",
            (IEnumerable<decimal> list, FilterOperator.NotInKey) =>
                $"{Field} not in ({string.Join(", ", list)})",
            (IEnumerable<DateTimeOffset> list, FilterOperator.InKey) =>
                $"{Field} in ({string.Join(", ", list.Select(i => $"\"{i}\""))})",
            (IEnumerable<DateTimeOffset> list, FilterOperator.NotInKey) =>
                $"{Field} not in ({string.Join(", ", list.Select(i => $"\"{i}\""))})",
            (IEnumerable<DateTime> list, FilterOperator.InKey) =>
                $"{Field} in ({string.Join(", ", list.Select(i => $"\"{i}\""))})",
            (IEnumerable<DateTime> list, FilterOperator.NotInKey) =>
                $"{Field} not in ({string.Join(", ", list.Select(i => $"\"{i}\""))})",
            (Guid value, FilterOperator.EqKey) =>
                $"{Field} == \"{value}\"",
            (Guid value, FilterOperator.NeKey) =>
                $"{Field} != \"{value}\"",
            (DateTimeOffset value, FilterOperator.EqKey) =>
                $"{Field} == \"{value}\"",
            (DateTimeOffset value, FilterOperator.NeKey) =>
                $"{Field} != \"{value}\"",
            (DateTime value, FilterOperator.EqKey) =>
                $"{Field} == \"{value}\"",
            (DateTime value, FilterOperator.NeKey) =>
                $"{Field} != \"{value}\"",
            (string value, FilterOperator.EqKey) =>
                $"{Field} == \"{value}\"",
            (string value, FilterOperator.NeKey) =>
                $"{Field} != \"{value}\"",
            (string value, FilterOperator.ContainsKey) =>
                $"{Field} like \"%{value}%\"",
            (string value, FilterOperator.StartsWithKey) =>
                $"{Field} like \"{value}%\"",
            (string value, FilterOperator.EndsWithKey) =>
                $"{Field} like \"%{value}\"",
            (bool value, FilterOperator.EqKey) =>
                $"{Field} == {value}",
            (bool value, FilterOperator.NeKey) =>
                $"{Field} != {value}",
            (int or uint or long or ulong or short or ushort or float or double or decimal, FilterOperator.EqKey) =>
                $"{Field} == {val}",
            (int or uint or long or ulong or short or ushort or float or double or decimal, FilterOperator.NeKey) =>
                $"{Field} != {val}",
            (int or uint or long or ulong or short or ushort or float or double or decimal, FilterOperator.GtKey) =>
                $"{Field} > {val}",
            (int or uint or long or ulong or short or ushort or float or double or decimal, FilterOperator.GteKey) =>
                $"{Field} >= {val}",
            (int or uint or long or ulong or short or ushort or float or double or decimal, FilterOperator.LtKey) =>
                $"{Field} < {val}",
            (int or uint or long or ulong or short or ushort or float or double or decimal, FilterOperator.LteKey) =>
                $"{Field} <= {val}",
            _ => throw new NotSupportedException($"Unsupported combination: The type of the value with '{Operator}' operator"),
        };

        return cond;
    }
}

file static class ObjectHelper
{
    extension(object? input)
    {
        public object? ExtractStrongType()
        {
            // 1. If it's not a JsonElement, it's already a native type (return it)
            if (input is null
             || input is not JsonElement element)
            {
                return input;
            }

            // 2. Evaluate the JSON type node
            return element.ValueKind switch
            {
                // Extracts text as a string, Guid, or DateTime Offset
                JsonValueKind.String => element.TryGetDateTimeOffset(out var dto)
                                      ? dto
                                      : element.TryGetDateTime(out var dt)
                                      ? dt
                                      : element.TryGetGuid(out var guid)
                                      ? guid
                                      : element.GetString(),

                // Parses numeric values accurately down to their smallest runtime footprint
                JsonValueKind.Number => element.TryGetUInt16(out var ushortVal)
                                      ? ushortVal
                                      : element.TryGetInt16(out var shortVal)
                                      ? shortVal
                                      : element.TryGetUInt32(out var uintVal)
                                      ? uintVal
                                      : element.TryGetInt32(out var intVal)
                                      ? intVal
                                      : element.TryGetUInt64(out var ulongVal)
                                      ? ulongVal
                                      : element.TryGetInt64(out var longVal)
                                      ? longVal                                    
                                      : element.TryGetDecimal(out var decimalVal)
                                      ? decimalVal
                                      : element.TryGetDouble(out var doubleVal)
                                      ? doubleVal
                                      : element.TryGetSingle(out var floatVal)
                                      ? floatVal
                                      : null,

                // Boolean mappings
                JsonValueKind.True => true,
                JsonValueKind.False => false,

                // Null representations
                JsonValueKind.Null => null!,

                // Complex types (Objects/Arrays) require full deserialization schemas
                JsonValueKind.Array => JsonSerializer.Deserialize<object>(element.GetRawText()),

                _ => throw new NotSupportedException($"Unknown JSON type node: {element.ValueKind}"),
            };
        }
    }
}
