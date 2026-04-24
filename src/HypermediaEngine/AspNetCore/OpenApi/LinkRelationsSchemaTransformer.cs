using Ardalis.SmartEnum;

using HypermediaEngine.Responses;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

using System.Text.Json.Nodes;

namespace HypermediaEngine.OpenApi;

internal sealed class LinkRelationsSchemaTransformer
    : IOpenApiSchemaTransformer
{
    public async Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        Type type = context.JsonTypeInfo.Type;
        if (!type.Name.Equals(typeof(LinkRelations).Name, StringComparison.Ordinal))
        {
            return;
        }

        OpenApiSchema linkRelationsSchema = await context
            .GetOrCreateSchemaAsync(
                typeof(SmartEnum<LinkRelations, int>),
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        // Clear the default object properties
        schema.Properties = null;
        schema.Type = JsonSchemaType.String;
        schema.Title = type.Name;
        schema.Enum = [.. LinkRelations.List.Select(ps => (JsonNode)JsonValue.Create(ps.Name))];
        schema.Example = linkRelationsSchema.Example;
        context.AddSchema(type, schema);
    }
}

