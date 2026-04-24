using LanguageExt;
using LanguageExt.Common;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace HypermediaEngine.Helpers;

internal static class LinkGeneratorHelpers
{
    extension(LinkGenerator linkGenerator)
    {
        /// <summary>
        /// Generates a URI for the specified endpoint using the provided HTTP context and optional route values.
        /// </summary>
        /// <remarks>If the generated URI is null or empty, the method returns None. The method requires
        /// both a valid HTTP context and a non-empty endpoint name to generate a URI.</remarks>
        /// <param name="httpContext">The HTTP context used to generate the URI. This parameter cannot be null.</param>
        /// <param name="endpointName">The name of the endpoint for which to generate the URI. This parameter cannot be null or whitespace.</param>
        /// <param name="values">An optional object containing route values to include in the generated URI. If not provided, default values
        /// will be used.</param>
        /// <returns>An optional result containing the generated URI as a string. Returns None if the URI cannot be generated.</returns>
        public OptionalResult<string> GenerateUri(
            HttpContext httpContext,
            string endpointName,
            object? values = null)
        {
            if (httpContext is null)
            {
                return new OptionalResult<string>(
                    new ArgumentNullException(nameof(httpContext)));
            }

            if (string.IsNullOrWhiteSpace(endpointName))
            {
                return new OptionalResult<string>(
                    new ArgumentNullException(nameof(endpointName), "Endpoint name cannot be null or whitespace."));
            }

            string? uri = values is null
                ? linkGenerator.GetUriByName(
                        httpContext,
                        endpointName,
                        new
                        {
                            version = "1"
                        })
                : linkGenerator.GetUriByName(
                        httpContext,
                        endpointName,
                        values);
            return string.IsNullOrWhiteSpace(uri)
                ? Option<string>.None
                : uri;
        }
    }
}
