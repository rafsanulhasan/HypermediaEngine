using Microsoft.OpenApi;

using System.Text.Json.Nodes;

namespace HypermediaEngine.Helpers;

public static class OpenApiResponseHelpers
{
    extension(IOpenApiResponse? response)
    {
        public IOpenApiResponse UpsertContentSchema(
            string contentType,
            OpenApiSchema schema,
            JsonNode? example = null,
            bool withExample = true)
        {
            ArgumentNullException.ThrowIfNull(response);
            ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
            ArgumentNullException.ThrowIfNull(schema);
            if (response.Content is null)
            {
                response = new OpenApiResponse()
                {
                    Content = new Dictionary<string, OpenApiMediaType>(StringComparer.OrdinalIgnoreCase)
                    {
                        {
                            contentType,
                            new OpenApiMediaType()
                            {
                                 Schema = schema,
                                 Example = withExample ? example : null,
                            }
                        },
                    },
                    Headers = response.Headers,
                    Description = response.Description,
                    Extensions = response.Extensions,
                    Links = response.Links,
                };
                return response;
            }

            if (response.Content.ContainsKey(contentType))
            {
                response.Content[contentType].Schema = schema;
                response.Content[contentType].Example = withExample ? example : null;
                if (!response.Content[contentType].Examples?.Any(e => e.Value == example) ?? false)
                {
                    response.Content[contentType].Examples?.Add(contentType, new OpenApiExample() { Value = example });
                }
                return response;
            }

            response.Content[contentType] = new OpenApiMediaType()
            {
                Schema = schema,
                Example = withExample ? example : null,
            };
            return response;
        }
    }
}
