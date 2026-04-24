using HypermediaEngine.Builders;

using Microsoft.Extensions.DependencyInjection;

namespace HypermediaEngine;

/// <summary>
/// Dependency registration for Hypermedia Engine services and configurations.
/// </summary>
public static class DependencyRegistration
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the hypermedia engine and configures endpoint routing for the application, enabling enhanced link
        /// generation and routing capabilities.
        /// </summary>
        /// <remarks>This method configures routing to not append trailing slashes and to use lowercase
        /// for URLs and query strings by default. It also registers services required for link generation. Call this
        /// method during application startup to ensure proper routing and hypermedia support.</remarks>
        /// <param name="configureBuilder">An optional action to configure <see cref="HypermediaEngineRegistrationBuilder"/>.</param>
        /// <returns>The updated IServiceCollection instance, enabling further service registration through method chaining.</returns>
        public IServiceCollection RegisterHypermediaEngineToEndpoints(
            Action<HypermediaEngineRegistrationBuilder>? configureBuilder = null)
        {
            HypermediaEngineRegistrationBuilder hypermediaRegistrationBuilder = new HypermediaEngineRegistrationBuilder(services)
                                                                                    .WithObjectResponseHandlers()
                                                                                    .WithCollectionResponseHandlers()
                                                                                    .WithResponseHandlersResolver()
                                                                                    .WithResponseBuilders();
            configureBuilder?.Invoke(hypermediaRegistrationBuilder);
            if (!hypermediaRegistrationBuilder.IsRoutingConfigured)
            {
                hypermediaRegistrationBuilder.WithRouteOptions();
            }
            if (!hypermediaRegistrationBuilder.IsLinkGenerationServiceRegistered)
            {
                hypermediaRegistrationBuilder.WithDefaultLinkGenerationService();
            }
            return hypermediaRegistrationBuilder.Build();
        }
    }
}
