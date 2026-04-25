using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace HypermediaEngine.OpenApi.SchemaTransformers;

internal sealed class SmartEnumSchemaTransformer
    : IOpenApiSchemaTransformer
{
    public async Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        Type type = context.JsonTypeInfo.Type;
        if (!ShouldProcessType(type, type.BaseType))
        {
            return;
        }

        OpenApiSchema offsetPagingSchema = await context
            .GetOrCreateSchemaAsync(
                type.BaseType,
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        // Clear the default object properties
        schema.Properties = null;
        schema.Type = JsonSchemaType.String;
        schema.Title = type.Name;
        PropertyInfo? listProp = type.BaseType.GetProperty("List", BindingFlags.Public | BindingFlags.Static);
        if (listProp is null)
        {
            return;
        }
        if (listProp.GetGetMethod() is not MethodInfo getMetod)
        {
            return;
        }

        IEnumerable? listOfValuesObj = (IEnumerable?)getMetod.Invoke(null, []);
        if (listOfValuesObj is null)
        {
            return;
        }
        schema.Enum = [
            ..listOfValuesObj
                    .Cast<object>()
                    .Select(value =>
                    {
                        string? name = value
                            .GetType()
                            .GetProperty("Name")?
                            .GetValue(value)?
                            .ToString();
                        return name;
                    })
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Select(n => n!),
            ];
        context.AddSchema(type, schema);
    }

    private static bool ShouldProcessType(Type type, [NotNullWhen(true)] Type? baseType)
    {
        return type is { BaseType: Type bt }
            && bt.Equals(baseType)
            && bt.Name.StartsWith("SmartEnum", StringComparison.Ordinal);
    }
}

