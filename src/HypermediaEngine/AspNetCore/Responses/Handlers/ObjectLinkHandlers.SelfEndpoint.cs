using Ardalis.GuardClauses;

using HypermediaEngine.Abstractions;
using HypermediaEngine.Helpers;

using LanguageExt.Common;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace HypermediaEngine.Responses.Handlers;

internal sealed class ObjectSelfEndpointLinkHandler<T>(
    ILinkGenerationService linkGenerationService,
    IHttpContextAccessor contextAccessor,
    ILogger<ObjectSelfEndpointLinkHandler<T>> logger
) : AbstractObjectLinkHandler<T>(contextAccessor) where T : notnull
{
    public override IHypermediaObjectBuilder<T> Handle(IEnumerable<HateoasLinkAttribute> attributes)
    {
        Guard.Against.Null(HttpContext, message: "HttpContext cannot be null");
        Guard.Against.Null(Builder, message: "Builder cannot be null");

        Endpoint endpoint = HttpContext.GetEndpoint()
                         ?? throw new InvalidOperationException("No endpoint found in the current context.");
        string? endpointName = endpoint.Metadata
                                .GetMetadata<EndpointNameMetadata>()?
                                .EndpointName
                         ?? throw new InvalidOperationException("Route metadata not found");
        RouteValueDictionary routeValues = HttpContext.GetRouteData().Values;

        OptionalResult<HypermediaLink> self = linkGenerationService.GenerateSelf<T>(endpointName, routeValues);
        Builder = self.Match(
            l => Builder.WithSelfLink(l),
            () => Builder,
            ex =>
            {
                logger.LogWarning(
                    "Failed to generate self link for endpoint '{EndpointName}' with route values {RouteValues}. Error: {ErrorMessage}",
                    endpointName,
                    routeValues
                        .Where(kv => kv.Value is not null)
                        .ToDictionary(
                            kv => kv.Key,
                            kv => kv.Value!.ToString(),
                            StringComparer.Ordinal),
                    ex.Message
                );
                return Builder;
            }
        );

        return Builder;
    }
}
