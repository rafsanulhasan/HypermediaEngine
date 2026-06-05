using HypermediaEngine.Responses;

using LanguageExt.Common;

using Microsoft.AspNetCore.Http;

using SynergyFx.HypermediaEngine.Abstractions;
using SynergyFx.HypermediaEngine.Attributes;

namespace SynergyFx.HypermediaEngine.Responses.Handlers;

internal sealed class ObjectStateTransitionEndpointLinkHandler<T>(
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
            throw new InvalidOperationException("ObjectBuilder is null");
        }

        IEnumerable<HateoasStateTransitionAttribute> stateTransitionLinkAttributes = attributes.OfType<HateoasStateTransitionAttribute>();
        foreach (HateoasStateTransitionAttribute st in stateTransitionLinkAttributes)
        {
            OptionalResult<HypermediaLink> stLink = linkGenerationService.GenerateLink(
                st.Rel,
                st.Method,
                HttpContext,
                st.RouteName);
            stLink.IfSucc(link => Builder.WithStateTransitionLink(link));
        }
        return Builder;
    }
}
