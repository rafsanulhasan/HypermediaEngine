using Ardalis.SmartEnum;

using HypermediaEngine.Requests.Paging;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

using System.Text.Json.Nodes;

namespace HypermediaEngine.OpenApi;

internal sealed class PagingStylesSchemaTransformer
    : IOpenApiSchemaTransformer
{
    public async Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        Type type = context.JsonTypeInfo.Type;
        if (!type.Name.Equals(nameof(PagingStyles), StringComparison.Ordinal))
        {
            return;
        }

        OpenApiSchema pagingStylesSchema = await context
            .GetOrCreateSchemaAsync(
                typeof(SmartEnum<PagingStyles, int>),
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        // Clear the default object properties
        schema.Properties = null;
        schema.Type = JsonSchemaType.String;
        schema.Title = type.Name;
        schema.Enum = [.. PagingStyles.List.Select(ps => (JsonNode)JsonValue.Create(ps.Name))];
        schema.Example = pagingStylesSchema.Example;
        context.AddSchema(type, schema);
    }
}
