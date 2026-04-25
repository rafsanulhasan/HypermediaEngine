using HypermediaEngine.Helpers;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

using System.Globalization;
using System.Net.Mime;

namespace HypermediaEngine.OpenApi.OperationTransformers;

internal sealed class JsonOperationTransformer<T>(bool isList, bool withExample)
        : JsonOperationTransformer(typeof(T), isList, withExample)
{
}

internal class JsonOperationTransformer : AbstractHalOperationTransformer
{
    public JsonOperationTransformer(Type type, bool isList, bool withExample) 
        : base(type, isList, withExample)
    {
        ConcreteType = type;
        ResponseType = isList
                     ? typeof(List<>).MakeGenericType(type)
                     : type;
    }

    public override string ContentType => MediaTypeNames.Application.Json;

    public override async Task TransformAsync(
        string docVersion,
        string apiVersion,
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (operation is null)
        {
            return;
        }
        int statusCode = StatusCodes.Status200OK;
        string statusCodeStr = statusCode.ToString(CultureInfo.InvariantCulture);
        if (!operation.TryGetResponse(statusCode, out IOpenApiResponse? response))
        {
            return;
        }

        OpenApiSchema responseSchema = await context
            .GetOrCreateSchemaAsync(
                ResponseType,
                new ApiParameterDescription()
                {
                    Name = ResponseType.Name,
                    Type = ResponseType,
                },
                cancellationToken)
            .ConfigureAwait(false);
        IOpenApiResponse r = response.UpsertContentSchema(MediaType, responseSchema, responseSchema.Example, WithExample);
        operation.Responses![statusCodeStr] = r;

        if (context.Document is { Components.Schemas: IDictionary<string, IOpenApiSchema> schemas })
        {
            schemas.Add(ResponseType.Name, responseSchema);
        }
    }
}
