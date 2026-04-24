using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace HypermediaEngine.OpenApi;

internal static class OpenApiSchemaTransformerContextHelpers
{
    extension(OpenApiSchemaTransformerContext context)
    {
        public void AddSchema(
            Type type,
            OpenApiSchema schema)
        {
            if (context.Document is not OpenApiDocument doc)
            {
                return;
            }
            doc.Components ??= new OpenApiComponents();
            doc.Components.Schemas ??= new Dictionary<string, IOpenApiSchema>(StringComparer.Ordinal);            
            doc.Components.Schemas[type.Name] = schema;
        }
    }
}
