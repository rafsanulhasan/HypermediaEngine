using HypermediaEngine.Abstractions;
using HypermediaEngine.Attributes;

using LanguageExt.Common;

using Microsoft.AspNetCore.Http;

namespace HypermediaEngine.Responses.Handlers;

internal sealed class ObjectRelatedEndpointLinkHandler<T>(
    ILinkGenerationService linkGenerationService,
    IHttpContextAccessor contextAccessor
) : AbstractObjectLinkHandler<T>(contextAccessor) where T : notnull
{
    public override IHypermediaObjectBuilder<T> Handle(IEnumerable<HateoasLinkAttribute> attributes)
    {
        if (HttpContext is null)
        {
            throw new InvalidOperationException("HttpContext is null");
        }

        if (Builder is null)
        {
            throw new InvalidOperationException("Builder is null");
        }

        IEnumerable<HateoasRelatedAttribute> relatedLinkAttributes = attributes.OfType<HateoasRelatedAttribute>();
        foreach (HateoasRelatedAttribute r in relatedLinkAttributes)
        {
            OptionalResult<HypermediaLink> rLink = linkGenerationService.GenerateLink(
                r.Rel,
                r.Method,
                HttpContext,
                r.RouteName);
            rLink.IfSucc(link => Builder.WithRelatedLink(link));
        }
        return Builder;
    }
}
