using HypermediaEngine.Abstractions;
using HypermediaEngine.Helpers;
using HypermediaEngine.Responses;

using LanguageExt;
using LanguageExt.Common;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace HypermediaEngine.Services;

/// <inheritdoc />
internal sealed class LinkGenerationService(
    LinkGenerator linkGenerator,
    IHttpContextAccessor httpContextAccessor,
    ILogger<LinkGenerationService> logger
) : ILinkGenerationService
{
    /// <inheritdoc />
    public Option<HttpContext> HttpContext { get; internal init; } = httpContextAccessor.HttpContext;

    /// <inheritdoc />
    public OptionalResult<HypermediaLink> GenerateLink(
        string endpointName,
        string httpMethod,
        object? routeValues,
        string? type = null,
        string? rel = null,
        string? title = null)
    {
        OptionalResult<HypermediaLink> link = HttpContext.Match(
            Some: httpContext => linkGenerator
                .GenerateUri(httpContext, endpointName, routeValues)
                .Match(
                    Some: uri => new HypermediaLink(uri, httpMethod, rel, type, title),
                    None: () => Option<HypermediaLink>.None,
                    ex =>
                    {
                        logger.LogError(ex, "Error generating link for endpoint '{EndpointName}'.", endpointName);
                        return new OptionalResult<HypermediaLink>(ex);
                    }),
            None: () =>
            {
                logger.LogError("Unable to generate link for endpoint '{EndpointName}'. No HttpContext available.", endpointName);
                return new OptionalResult<HypermediaLink>(
                    new InvalidOperationException("Unable to generate link. No HttpContext available."));
            });
        return link;
    }
}
