using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace HypermediaEngine.OpenApi.SchemaTransformers;

internal sealed class OptionSchemaTransformer
    : IOpenApiSchemaTransformer
{
    public async Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        Type type = context.JsonTypeInfo.Type;
        if (!type.IsGenericType)
        {
            return;
        }

        bool isOption = type.Name.StartsWith("Option", StringComparison.OrdinalIgnoreCase);

        if (!isOption)
        {
            return;
        }

        Type genericArgument = type.GetGenericArguments()[0];
        OpenApiSchema typeSchema = await context
            .GetOrCreateSchemaAsync(genericArgument, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        OpenApiSchema nullSchema = new()
        {
            Type = JsonSchemaType.Null,
        };

        // Clear the default object properties
        schema.Properties = null;
        schema.Type = JsonSchemaType.Object | JsonSchemaType.Null;
        schema.AnyOf = [ typeSchema, nullSchema ];
        schema.Title = $"Optional{genericArgument.Name}";
    }
}
