using HypermediaEngine.Requests.Paging;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace HypermediaEngine.OpenApi;

internal sealed class OffsetOrCursorPagingSchemaTransformer
    : IOpenApiSchemaTransformer
{
    public async Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        Type type = context.JsonTypeInfo.Type;
        if (!type.Name.Equals(nameof(OffsetOrCursorPaging), StringComparison.Ordinal))
        {
            return;
        }

        OpenApiSchema offsetPagingSchema = await context
            .GetOrCreateSchemaAsync(
                typeof(OffsetPaging),
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        OpenApiSchema cursorPagingSchema = await context
            .GetOrCreateSchemaAsync(
                typeof(CursorPaging),
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        // Clear the default object properties
        schema.Properties = null;
        schema.Type = JsonSchemaType.Object;
        schema.Title = type.Name;
        schema.OneOf = [offsetPagingSchema, cursorPagingSchema];
        schema.Example = offsetPagingSchema.Example;
        if (offsetPagingSchema.Example is not null
         && cursorPagingSchema.Example is not null)
            schema.Examples = [offsetPagingSchema.Example, cursorPagingSchema.Example];

       context.AddSchema(type, schema);
    }
}

