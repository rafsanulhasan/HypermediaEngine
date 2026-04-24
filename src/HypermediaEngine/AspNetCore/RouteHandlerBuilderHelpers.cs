using HypermediaEngine.Attributes;
using HypermediaEngine.Helpers;
using HypermediaEngine.Http;
using HypermediaEngine.Requests;
using HypermediaEngine.Responses;

using LanguageExt;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

using System.Net.Mime;

namespace HypermediaEngine;

public static class RouteHandlerBuilderHelpers
{
    extension(RouteHandlerBuilder builder)
    {
        public RouteHandlerBuilder ProducesHal<T>(
            int statusCode = StatusCodes.Status200OK,
            bool isList = false,
            bool isMarten = false
        ) where T : notnull
        {
            if (!isList)
            {
                builder
                    .Produces<HypermediaObjectResponse<T>>(
                        statusCode,
                        contentType: HalMediaTypeNames.Application.HalJson)
                    .AddHalResponsesOpenApiOperationTransformers<T>(
                        statusCode,
                        withHalExample: false,
                        withJsonExample: true);

                builder.Produces<T>(
                    statusCode,
                    contentType: MediaTypeNames.Application.Json);
            }
            else
            {
                Type listUnderlyingType = typeof(T);
                Type halResponseType = typeof(HypermediaCollectionResponse<>).MakeGenericType(listUnderlyingType);
                builder
                    .Produces(
                        statusCode,
                        responseType: halResponseType,
                        contentType: HalMediaTypeNames.Application.HalJson)
                    .AddHalResponsesOpenApiOperationTransformers(
                        listUnderlyingType,
                        statusCode,
                        withHalExample: false,
                        withJsonExample: true,
                        isList: true);

                builder.Produces<List<T>>(
                    statusCode,
                    contentType: MediaTypeNames.Application.Json);
            }
            builder
                .Produces(StatusCodes.Status404NotFound)
                .AddEndpointFilter<ProducesHalAttribute<T>>()
                .AddHalOpenApiOperationRequestParameterTransformer(false);
            return builder;
        }

        public RouteHandlerBuilder ProducesHalResponse<T>(int statusCode = StatusCodes.Status200OK)
            where T : class
        {
            builder
                .Produces<HypermediaObjectResponse<T>>(
                    statusCode,
                    contentType: HalMediaTypeNames.Application.HalJson)
                .Produces<T>(
                    statusCode,
                    contentType: MediaTypeNames.Application.Json)
                .Produces(StatusCodes.Status404NotFound)
                .AddEndpointFilter<ProducesHalResponseAttribute<T, HypermediaObjectResponse<T>>>()
                .AddHalResponsesOpenApiOperationTransformers<T>(
                    statusCode,
                    withHalExample: true,
                    withJsonExample: false)
                .AddHalOpenApiOperationRequestParameterTransformer(true);
            return builder;
        }

        public RouteHandlerBuilder WithStateTransitionLink(
            string name,
            string rel,
            string method,
            string routeName)
        {
            builder.WithMetadata(
                new HateoasStateTransitionAttribute(
                    name,
                    rel,
                    method,
                    routeName));
            return builder;
        }

        public RouteHandlerBuilder WithRelatedLink(
            string name,
            string rel,
            string method,
            string routeName)
        {
            builder.WithMetadata(
                new HateoasRelatedAttribute(
                    name,
                    rel,
                    method,
                    routeName));
            return builder;
        }

        public RouteHandlerBuilder WithPagingParams()
        {
            builder.AddOpenApiOperationTransformer((operation, ctx, ct) =>
            {
                operation.RequestBody??=new OpenApiRequestBody();
                operation.Parameters ??= [];
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "page",
                    In = ParameterLocation.Query,
                    Required = false,
                    AllowEmptyValue = false,
                    Description = "The page number to retrieve.",
                    Schema = new OpenApiSchema
                    {
                        Type = JsonSchemaType.Number,
                        Default = 1,
                    },
                });
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "pageSize",
                    In = ParameterLocation.Query,
                    Required = false,
                    AllowEmptyValue = false,
                    Description = "The number of items to retrieve per page.",
                    Schema = new OpenApiSchema
                    {
                        Type = JsonSchemaType.Number,
                        Default = 10,
                    },
                });
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "cursor",
                    In = ParameterLocation.Query,
                    Required = false,
                    AllowEmptyValue = true,
                    Description = "The cursor for pagination.",
                    Schema = new OpenApiSchema
                    {
                        Type = JsonSchemaType.String,
                    },
                });
                return Task.CompletedTask;
            });
            return builder;
        }
        public RouteHandlerBuilder WithFilterAndSortingParams()
        {
            builder.AddOpenApiOperationTransformer(async (operation, ctx, ct) =>
            {
                OpenApiSchema queryBodySchema = await ctx
                        .GetOrCreateSchemaAsync(typeof(QueryBody), null, ct)
                        .ConfigureAwait(false);
                operation.RequestBody = new OpenApiRequestBody()
                {
                    Content = new Dictionary<string, OpenApiMediaType>(StringComparer.Ordinal)
                    {
                        [MediaTypeNames.Application.Json] = new OpenApiMediaType()
                        {
                            Schema = queryBodySchema,
                        },
                    },
                    Required = false,
                };
                ctx.Document?.Components?.Schemas?[nameof(QueryBody)] = queryBodySchema;
            });
            return builder;
        }

        private RouteHandlerBuilder AddHalResponsesOpenApiOperationTransformers<T>(
            int statusCode,
            bool withHalExample,
            bool withJsonExample,
            bool isList = false
        ) where T : notnull
        {
            return builder.AddHalResponsesOpenApiOperationTransformers(
                typeof(T),
                statusCode,
                withHalExample,
                withJsonExample,
                isList);
        }

        private RouteHandlerBuilder AddHalResponsesOpenApiOperationTransformers(
            Type type,
            int statusCode,
            bool withHalExample,
            bool withJsonExample,
            bool isList = false
        )
        {
            return builder.AddOpenApiOperationTransformer(async (operation, ctx, ct) =>
            {
                string docVersion = ctx.DocumentName.Replace("v", string.Empty, StringComparison.Ordinal);
                string apiVersion = ctx
                    .GetImplementedApiVersions()
                    .Match(
                        implementedVersions => implementedVersions.FirstOrDefault(iv => iv.Equals(docVersion, StringComparison.Ordinal)) ?? string.Empty,
                        () => string.Empty);
                Task halTransformer = ctx.TransformHalResponseAsync(
                    isList
                        ? typeof(HypermediaCollectionResponse<>).MakeGenericType(type)
                        : typeof(HypermediaObjectResponse<>).MakeGenericType(type),
                    operation,
                    statusCode,
                    withHalExample,
                    apiVersion,
                    isList,
                    ct);
                Task jsonTransformer = ctx.TransformJsonResponseAsync(
                    isList
                        ? typeof(IEnumerable<>).MakeGenericType(type)
                        : type,
                    operation,
                    statusCode,
                    withJsonExample,
                    apiVersion,
                    ct);
                if (withHalExample)
                {
                    await Task.WhenAll(halTransformer, jsonTransformer).ConfigureAwait(false);
                    return;
                }

                await Task.WhenAll(jsonTransformer, halTransformer).ConfigureAwait(false);
            });
        }

        private RouteHandlerBuilder AddHalOpenApiOperationRequestParameterTransformer(bool halFirst)
        {
            return builder.AddOpenApiOperationTransformer(async (operation, ctx, ct) =>
            {
                HttpContext? httpCtx = ctx.ApplicationServices.GetRequiredService<IHttpContextAccessor>().HttpContext;
                string? requestMethod = httpCtx?.Request.Method;
                if (string.IsNullOrWhiteSpace(requestMethod))
                {
                    return;
                }

                if (!requestMethod.Equals(HttpMethods.Get, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
                operation.Parameters ??= [];
                string documentName = ctx.DocumentName.Replace("v", string.Empty, StringComparison.OrdinalIgnoreCase);
                Option<IEnumerable<string>> apiVersionsList = ctx
                    .GetImplementedApiVersions()
                    .Map(versions => versions.Where(v => v.Equals(documentName, StringComparison.OrdinalIgnoreCase)));
                await apiVersionsList
                    .IfSomeAsync(apiVersions =>
                    {
                        foreach (string apiVersion in apiVersions)
                        {
                            if (operation.Parameters.FirstOrDefault(p => p is { In: ParameterLocation.Header, Name: "accept" or "Accept" }) is OpenApiParameter apiParam)
                            {
                                operation.Parameters.Remove(apiParam);
                            }
                            operation.Parameters.Add(HalMediaTypeNames.CreateParameter(halFirst, apiVersion));
                        }
                    })
                    .ConfigureAwait(false);
            });
        }
    }
}
