using HypermediaEngine.OpenApi.SchemaTransformers;

using Microsoft.AspNetCore.OpenApi;

namespace HypermediaEngine.OpenApi;

public static class OpenApiOptionHelpers
{
    extension(OpenApiOptions options)
    {
        public OpenApiOptions RegisterHypermediaTransformers()
        {
            options.AddSchemaTransformer<OneOfSchemaTransformer>();
            //options.AddSchemaTransformer<OptionSchemaTransformer>();
            options.AddSchemaTransformer<SmartEnumSchemaTransformer>();
            return options;
        }
    }
}
