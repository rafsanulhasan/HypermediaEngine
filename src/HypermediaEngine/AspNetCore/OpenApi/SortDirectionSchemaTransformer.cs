using Ardalis.SmartEnum;

using HypermediaEngine.Requests.Sorting;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

using System.Text.Json.Nodes;

namespace HypermediaEngine.OpenApi;

internal sealed class SortDirectionSchemaTransformer
    : IOpenApiSchemaTransformer
{
    public async Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        Type type = context.JsonTypeInfo.Type;
        if (!type.Name.Equals(nameof(SortDirection), StringComparison.Ordinal))
        {
            return;
        }

        OpenApiSchema sortDirectionSchema = await context
            .GetOrCreateSchemaAsync(
                typeof(SmartEnum<SortDirection, int>),
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        // Clear the default object properties
        schema.Properties = null;
        schema.Type = JsonSchemaType.String;
        schema.Title = type.Name;
        schema.Enum = [.. SortDirection.List.Select(sd => (JsonNode)JsonValue.Create(sd.Name))];
        schema.Example = sortDirectionSchema.Example;
        context.AddSchema(type, schema);
    }
}
