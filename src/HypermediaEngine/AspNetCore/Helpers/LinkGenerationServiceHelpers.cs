using HypermediaEngine.Abstractions;
using HypermediaEngine.Responses;

using LanguageExt;
using LanguageExt.Common;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace HypermediaEngine.Helpers;

/// <summary>
/// Provides extension methods for generating hypermedia 'self' links using an ILinkGenerationService instance.
/// </summary>
/// <remarks>This static class offers helper methods to simplify the creation of 'self' hypermedia links in
/// RESTful APIs. The methods support generating links based on the current HTTP context or a specified endpoint name,
/// facilitating consistent hypermedia responses. These helpers are intended to be used within the context of a link
/// generation service implementation.</remarks>
public static class LinkGenerationServiceHelpers
{
    extension(ILinkGenerationService linkGenerationService)
    {
        /// <summary>
        /// Generates a hypermedia link that represents the 'self' relation for the specified resource endpoint.
        /// </summary>
        /// <typeparam name="T">The type of the resource for which the self link is being generated.</typeparam>
        /// <param name="endpointName">The name of the endpoint corresponding to the resource. This should match a defined route in the
        /// application.</param>
        /// <param name="values">An optional object containing route values to use when generating the link. May be null if no additional
        /// route values are required.</param>
        /// <param name="title">An optional title for the generated link, providing additional context or description. May be null.</param>
        /// <returns>An OptionalResult containing the generated self hypermedia link. If link generation fails, the result
        /// indicates the failure.</returns>
        public OptionalResult<HypermediaLink> GenerateSelf<T>(
            string endpointName,
            object? values = null,
            string? title = null)
        {
            return linkGenerationService.GenerateGetLink<T>(endpointName, values, LinkRelations.Self, title);
        }

        /// <summary>
        /// Generates a hypermedia 'self' link for the current endpoint, optionally including a title in the link
        /// metadata.
        /// </summary>
        /// <remarks>This method retrieves the current HTTP context and attempts to generate a 'self' link
        /// based on the endpoint's metadata and route values. Ensure that the HTTP context is available when calling
        /// this method.</remarks>
        /// <typeparam name="T">The type of the resource for which the self link is generated.</typeparam>
        /// <param name="title">An optional title to include in the generated link's metadata. If specified, the title is added to the link.</param>
        /// <returns>An optional result containing the generated hypermedia self link for the current endpoint. Returns None if
        /// the endpoint name is not available.</returns>
        public OptionalResult<HypermediaLink> GenerateSelf<T>(string? title = null)
        {
            OptionalResult<HypermediaLink> selfLink = linkGenerationService.HttpContext
                .Match(
                    Some: httpContext =>
                    {
                        Endpoint? endpoint = httpContext.GetEndpoint();
                        string endpointName = endpoint?.Metadata.GetMetadata<EndpointNameMetadata>()?.EndpointName ?? string.Empty;
                        if (string.IsNullOrWhiteSpace(endpointName))
                        {
                            return Option<HypermediaLink>.None;
                        }
                        object routeValues = httpContext.Request.RouteValues;
                        return linkGenerationService.GenerateGetLink<T>(endpointName, routeValues, LinkRelations.Self, title);
                    },
                    None: () => Option<HypermediaLink>.None);
            return selfLink;
        }
    }
}
