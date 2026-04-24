using HypermediaEngine.Requests;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace HypermediaEngine.OpenApi;

internal sealed class QueryParamsSchemaTransformer
    : IOpenApiSchemaTransformer
{
    public async Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        Type type = context.JsonTypeInfo.Type;
        if (!type.Name.Equals(nameof(QueryParams), StringComparison.Ordinal))
        {
            return;
        }

        OpenApiSchema queryParamsSchema = await context
            .GetOrCreateSchemaAsync(
                typeof(QueryParams),
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        // Clear the default object properties
        schema.Properties = queryParamsSchema.Properties;
        schema.Type = JsonSchemaType.Object;
        schema.Title = type.Name;
        schema.Example = queryParamsSchema.Example;
        context.AddSchema(type, queryParamsSchema);
    }
}
