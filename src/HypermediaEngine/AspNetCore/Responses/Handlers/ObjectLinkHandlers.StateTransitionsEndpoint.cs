using HypermediaEngine.Abstractions;
using HypermediaEngine.Attributes;

using LanguageExt.Common;

using Microsoft.AspNetCore.Http;

namespace HypermediaEngine.Responses.Handlers;

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
