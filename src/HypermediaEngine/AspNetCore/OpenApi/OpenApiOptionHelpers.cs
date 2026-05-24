using Microsoft.AspNetCore.OpenApi;

using SynergyFx.HypermediaEngine.OpenApi.SchemaTransformers;

namespace SynergyFx.HypermediaEngine.OpenApi;

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
