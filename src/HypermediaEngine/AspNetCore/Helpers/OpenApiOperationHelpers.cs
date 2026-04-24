using HypermediaEngine.Http;

using Microsoft.OpenApi;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Mime;

namespace HypermediaEngine.Helpers;

internal static class OpenApiOperationHelpers
{
    extension(OpenApiOperation operation)
    {
        public IOpenApiResponse GetOrCreateResponse(int statusCode, OpenApiResponse newResponse)
        {
            if (operation.Responses is not null
             && operation.Responses.TryGetValue(
                    statusCode.ToString(CultureInfo.InvariantCulture),
                    out IOpenApiResponse? response)
             && response is not null)
            {
                return response;
            }

            return newResponse;
        }

        public bool TryGetResponse(int statusCode, [NotNullWhen(true)] out IOpenApiResponse? response)
        {
            response = null;
            if (operation.Responses is not null
             && operation.Responses.TryGetValue(
                    statusCode.ToString(CultureInfo.InvariantCulture),
                    out response)
             && response is not null)
            {
                return true;
            }

            return false;
        }

        internal bool IsTransformedIntoHalResponse()
        {
            return operation.Responses is { Count: > 0 }
                && operation.Responses.TryGetValue("200", out IOpenApiResponse? response)
                && response is { Content.Count: > 0 }
                && response.Content.TryGetValue(HalMediaTypeNames.Application.HalJson, out OpenApiMediaType? mediaType)
                && mediaType is { Schema: not null };
        }

        internal bool IsTransformedIntoJsonResponse(string? version = null)
        {
            string contentType = string.IsNullOrWhiteSpace(version)
                               ? MediaTypeNames.Application.Json
                               : $"{MediaTypeNames.Application.Json}; v={version}";
            return operation.Responses is { Count: > 0 }
                && operation.Responses.TryGetValue("200", out IOpenApiResponse? response)
                && response is { Content.Count: > 0 }
                && response.Content.TryGetValue(contentType, out OpenApiMediaType? mediaType)
                && mediaType is not null;
        }
    }
}
