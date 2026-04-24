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

            options.AddSchemaTransformer<QueryParamsSchemaTransformer>();

            //options.AddSchemaTransformer<OffsetOrCursorPagingSchemaTransformer>();
            //options.AddSchemaTransformer<SortDirectionSchemaTransformer>();
            //options.AddSchemaTransformer<PagingStylesSchemaTransformer>();
            //options.AddSchemaTransformer<LinkRelationsSchemaTransformer>();
            return options;
        }
    }
}
