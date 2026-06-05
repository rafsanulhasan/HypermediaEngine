using Ardalis.GuardClauses;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace SynergyFx.HypermediaEngine.Requests.Filtering;

public abstract class AbstractFilterConditionHandler(IFilterConditionHandler innerHandler) : IFilterConditionHandler
{
    public string Handle(FilterCondition condition)
    {
        Guard.Against.Null(condition);
        condition = condition with
        {
            Value = ExtractStrongType(condition.Value),
        };
        if (ShouldHandle(condition, out object? value))
        {
            return HandleCondition(condition, value);
        }

        return innerHandler!.Handle(condition);
    }

    protected abstract string HandleCondition(FilterCondition condition, object? s);

    protected abstract bool ShouldHandle(
        FilterCondition condition,
        [NotNullWhen(true)] out object? value);

    protected object? ExtractStrongType(object? input)
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
            JsonValueKind.String => element.TryGetDateTimeOffset(out DateTimeOffset dto)
                                  ? dto
                                  : element.TryGetDateTime(out global::System.DateTime dt)
                                  ? dt
                                  : element.TryGetGuid(out global::System.Guid guid)
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
