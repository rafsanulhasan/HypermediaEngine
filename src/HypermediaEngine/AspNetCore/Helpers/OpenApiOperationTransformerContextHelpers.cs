using Asp.Versioning;

using EntityTagCaching.Models;

using HypermediaEngine.Http;
using HypermediaEngine.Responses;

using LanguageExt;

using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

using System.Globalization;
using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace HypermediaEngine.Helpers;

public static class OpenApiOperationTransformerContextHelpers
{
    extension(OpenApiOperationTransformerContext ctx)
    {
        public Option<ApiVersionMetadata> GetApiVersionMetadata()
        {
            if (ctx is { Description.ActionDescriptor.EndpointMetadata: not null }
             && ctx.Description.ActionDescriptor.EndpointMetadata
                   .OfType<ApiVersionMetadata>()
                   .FirstOrDefault() is ApiVersionMetadata metadata)
            {
                return metadata;
            }
            return Option<ApiVersionMetadata>.None;
        }

        public Option<ApiVersionModel> GetApiVersionModel(ApiVersionMapping mapping = ApiVersionMapping.Explicit)
        {
            return ctx.GetApiVersionMetadata().Map(
                metadata => metadata.Map(mapping));
        }

        public Option<IEnumerable<string>> GetImplementedApiVersions(ApiVersionMapping mapping = ApiVersionMapping.Explicit)
        {
            return ctx.GetApiVersionModel(mapping).Map(
                model => model.ImplementedApiVersions.Select(v => v.ToString()));
        }

        public Option<IEnumerable<string>> GetDeclaredApiVersions(ApiVersionMapping mapping = ApiVersionMapping.Explicit)
        {
            return ctx.GetApiVersionModel(mapping).Map(
                model => model.DeclaredApiVersions.Select(v => v.ToString()));
        }

        public Option<string> GetApiVersionString(ApiVersionMapping mapping = ApiVersionMapping.Explicit)
        {
            return ctx.GetApiVersionModel(mapping).Match(
                versionModel =>
                {
                    if (versionModel is { DeclaredApiVersions.Count: > 0 }
                     && versionModel.DeclaredApiVersions[0].ToString() is string apiVersion)
                    {
                        if (string.IsNullOrWhiteSpace(apiVersion))
                        {
                            return apiVersion;
                        }
                        else if (versionModel is { ImplementedApiVersions.Count: > 0 }
                              && versionModel.ImplementedApiVersions[0].ToString() is string apiVersion2)
                        {
                            return apiVersion2;
                        }
                        else
                        {
                            return Option<string>.None;
                        }
                    }
                    return Option<string>.None;
                },
                () => Option<string>.None);
        }

        public Task TransformHalResponseAsync<T>(
            OpenApiOperation operation,
            int statusCode,
            bool withExample,
            string apiVersion,
            CancellationToken ct = default
        ) where T : notnull
        {
            return ctx.TransformHalResponseAsync(
                typeof(T),
                operation,
                statusCode,
                withExample,
                apiVersion,
                false,
                ct);
        }

        public async Task TransformHalResponseAsync(
            Type responseType,
            OpenApiOperation operation,
            int statusCode,
            bool withExample,
            string apiVersion,
            bool isList = false,
            CancellationToken ct = default
        )
        {
            if (operation is null)
            {
                return;
            }
            string statusCodeStr = statusCode.ToString(CultureInfo.InvariantCulture);
            string contentType = HalMediaTypeNames.AppendVersionToMediaType(HalMediaTypeNames.Application.HalJson, apiVersion);
            Type underlyingType = isList
                              ? typeof(IEnumerable<>).MakeGenericType(responseType.GetGenericArguments()[0])
                              : responseType.GetGenericArguments()[0];
            string underlyingTypeSchemaName = isList
                                            ? $"ListOf{underlyingType.GetGenericArguments()[0].Name}"
                                            : underlyingType.Name;
            string responseSchemaName = $"ResponseOf{underlyingTypeSchemaName}";
            OpenApiSchema responseSchema = await ctx
                .GetOrCreateSchemaAsync(
                    responseType,
                    new ApiParameterDescription()
                    {
                        Name = responseSchemaName,
                        Type = responseType,
                    },
                    ct)
                .ConfigureAwait(false);
            OpenApiSchema underlyingTypeSchema = await ctx
                .GetOrCreateSchemaAsync(
                    underlyingType,
                    new ApiParameterDescription()
                    {
                        Name = underlyingTypeSchemaName,
                        Type = underlyingType,
                    },
                    ct)
                .ConfigureAwait(false);
            EntityTag entityTag = EntityTag.Empty;
            "EntityTag"
                .GetETag()
                .IfSucc(etag => EntityTag
                    .From(etag)
                    .IfSome(t => entityTag = t));
            JsonNode? example = new JsonObject()
            {
                ["links"] = isList
                          ? JsonSerializer.SerializeToNode<ListLinkCollection>(new())
                          : JsonSerializer.SerializeToNode<ObjectLinkCollection>(new()),
                ["meta"] = JsonSerializer.SerializeToNode<ResponseMetadata>(isList
                         ? new ListResponseMetadata(entityTag)
                         : new ObjectResponseMetadata(entityTag)),
            };
            if (isList)
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
                            {
                                contentType,
                                new()
                                {
                                    Schema = responseSchema,
                                    Example = example,
                                }
                            },
                        },
                    },
                };
                return;
            }
            IOpenApiResponse openApiResponse = response.UpsertContentSchema(contentType, responseSchema, example, withExample);
            operation.Responses![statusCodeStr] = openApiResponse;

            if (ctx.Document is { Components.Schemas: IDictionary<string, IOpenApiSchema> schemas })
            {
                schemas.Add(underlyingTypeSchemaName, underlyingTypeSchema);
                schemas.Add(responseSchemaName, responseSchema);
            }
        }

        public Task TransformJsonResponseAsync<T>(
            OpenApiOperation operation,
            int statusCode,
            bool withExample,
            string apiVersion,
            CancellationToken ct)
        {
            return ctx.TransformJsonResponseAsync(
                typeof(T),
                operation,
                statusCode,
                withExample,
                apiVersion,
                ct);
        }

        public async Task TransformJsonResponseAsync(
            Type type,
            OpenApiOperation operation,
            int statusCode,
            bool withExample,
            string apiVersion,
            CancellationToken ct)
        {
            if (operation is null)
            {
                return;
            }
            string contentType = HalMediaTypeNames.AppendVersionToMediaType(
                                    MediaTypeNames.Application.Json,
                                    apiVersion);
            string statusCodeStr = statusCode.ToString(CultureInfo.InvariantCulture);
            if (!operation.TryGetResponse(statusCode, out IOpenApiResponse? response))
            {
                return;
            }

            Type responseType = type;
            OpenApiSchema responseSchema = await ctx
                .GetOrCreateSchemaAsync(
                    responseType,
                    new ApiParameterDescription()
                    {
                        Name = responseType.Name,
                        Type = responseType,
                    },
                    ct)
                .ConfigureAwait(false);
            IOpenApiResponse r = response.UpsertContentSchema(contentType, responseSchema, responseSchema.Example, withExample);
            operation.Responses![statusCodeStr] = r;
        }
    }
}
