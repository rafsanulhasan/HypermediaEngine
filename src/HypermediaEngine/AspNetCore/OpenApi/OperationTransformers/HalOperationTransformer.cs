using EntityTagCaching.Models;

using HypermediaEngine.Helpers;
using HypermediaEngine.Http;
using HypermediaEngine.Responses;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace HypermediaEngine.OpenApi.OperationTransformers;

internal sealed class HalOperationTransformer<T>(bool isList, bool withExample)
        : HalOperationTransformer(typeof(T), isList, withExample)
{
}

internal class HalOperationTransformer(Type type, bool isList, bool withExample)
        : AbstractHalOperationTransformer(type, isList, withExample)
{
    public override string ContentType => HalMediaTypeNames.Application.HalJson;

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
        Type underlyingType = IsList
                          ? typeof(List<>).MakeGenericType(ResponseType.GetGenericArguments()[0])
                          : ResponseType.GetGenericArguments()[0];
        string underlyingTypeSchemaName = IsList
                                        ? $"ListOf{underlyingType.GetGenericArguments()[0].Name}"
                                        : underlyingType.Name;
        string responseSchemaName = $"ResponseOf{underlyingTypeSchemaName}";
        OpenApiSchema responseSchema = await context
            .GetOrCreateSchemaAsync(
                ResponseType,
                new ApiParameterDescription()
                {
                    Name = responseSchemaName,
                    Type = ResponseType,
                },
                cancellationToken)
            .ConfigureAwait(false);
        OpenApiSchema underlyingTypeSchema = await context
            .GetOrCreateSchemaAsync(
                underlyingType,
                new ApiParameterDescription()
                {
                    Name = underlyingTypeSchemaName,
                    Type = underlyingType,
                },
                cancellationToken)
            .ConfigureAwait(false);
        EntityTag entityTag = EntityTag.Empty;
        "EntityTag"
            .GetETag()
            .IfSucc(etag => EntityTag
                .From(etag)
                .IfSome(t => entityTag = t));
        JsonNode? example = new JsonObject()
        {
            ["links"] = IsList
                      ? JsonSerializer.SerializeToNode<ListLinkCollection>(new())
                      : JsonSerializer.SerializeToNode<ObjectLinkCollection>(new()),
            ["meta"] = JsonSerializer.SerializeToNode<ResponseMetadata>(IsList
                     ? new ListResponseMetadata(entityTag)
                     : new ObjectResponseMetadata(entityTag)),
        };
        if (IsList)
        {
            example["items"] = new JsonArray(underlyingTypeSchema.Example);
        }
        else
        {
            example["data"] = underlyingTypeSchema.Example;
        }
        example = null;
        if (!operation.TryGetResponse(statusCode, out IOpenApiResponse? response))
        {
            operation.Responses = new OpenApiResponses()
            {
                [statusCodeStr] = new OpenApiResponse()
                {
                    Content = new Dictionary<string, OpenApiMediaType>(StringComparer.OrdinalIgnoreCase)
                    {
                        [MediaType] = new()
                        {
                            Schema = responseSchema,
                            Example = example,
                        },
                    },
                },
            };
            return;
        }
        IOpenApiResponse openApiResponse = response.UpsertContentSchema(MediaType, responseSchema, example, WithExample);
        operation.Responses![statusCodeStr] = openApiResponse;

        if (context.Document is { Components.Schemas: IDictionary<string, IOpenApiSchema> schemas })
        {
            schemas.Add(underlyingTypeSchemaName, underlyingTypeSchema);
            schemas.Add(responseSchemaName, responseSchema);
        }
    }
}
