using HypermediaEngine.Abstractions;
using HypermediaEngine.Responses.Handlers;
using HypermediaEngine.Services;

using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace HypermediaEngine.Builders;

/// <summary>
/// Provides a builder for configuring and registering services required for hypermedia support in an ASP.NET Core
/// application.
/// </summary>
/// <remarks>Use this builder to add and configure routing options, link generation services, and other
/// dependencies necessary for enabling hypermedia features. The builder is typically used during application startup to
/// ensure that all required services are registered in the dependency injection container before the application begins
/// handling requests.</remarks>
/// <remarks>
/// Initializes a new instance of the HypermediaEngineRegistrationBuilder class and configures the required services
/// for hypermedia support.
/// </remarks>
/// <remarks>This constructor adds the IHttpContextAccessor to the provided service collection, enabling
/// access to the current HTTP context throughout the application.</remarks>
/// <param name="services">The service collection to which hypermedia-related services will be registered. Cannot be null.</param>
public sealed class HypermediaEngineRegistrationBuilder(IServiceCollection services)
{
    internal static readonly Action<RouteOptions> DefaultRoutingOptions = options =>
    {
        options.AppendTrailingSlash = false;
        options.LowercaseUrls = true;
        options.LowercaseQueryStrings = true;
    };

    internal bool IsRoutingConfigured { get; private set; }
    internal bool IsHttpContextAccessorAdded { get; private set; } = true;
    internal bool IsLinkGenerationServiceRegistered { get; private set; }

    internal IServiceCollection Services { get; private set; } = services.AddHttpContextAccessor();

    /// <summary>
    /// Configures routing options for the hypermedia engine using the specified configuration action.
    /// </summary>
    /// <remarks>Routing options are only configured if they have not been set previously. Subsequent calls to
    /// this method have no effect once routing is configured.</remarks>
    /// <param name="configureRouteOptions">An action that receives a <see cref="T:Microsoft.AspNetCore.Routing.RouteOptions"/> instance to configure routing parameters. This parameter
    /// can be <see langword="null"/> if no additional configuration is required.</param>
    /// <returns>The current <see cref="HypermediaEngineRegistrationBuilder"/> instance, enabling method chaining.</returns>
    public HypermediaEngineRegistrationBuilder WithRouteOptions(Action<RouteOptions>? configureRouteOptions = null)
    {
        if (IsRoutingConfigured) return this;
        Services = Services.AddRouting(options =>
        {
            DefaultRoutingOptions.Invoke(options);
            configureRouteOptions?.Invoke(options);
        });
        IsRoutingConfigured = true;
        return this;
    }

    /// <summary>
    /// Registers a link generation service with the hypermedia engine, enabling the generation of hypermedia links
    /// using the specified implementation.
    /// </summary>
    /// <remarks>This method ensures that the link generation service is registered only once. Subsequent
    /// calls have no effect if the service is already registered.</remarks>
    /// <typeparam name="TLinkGenerationService">The type of the link generation service to register. Must implement the ILinkGenerationService interface.</typeparam>
    /// <param name="implementationFactory">A factory function that creates an instance of the link generation service. The function receives an
    /// IServiceProvider for resolving dependencies.</param>
    /// <returns>The current HypermediaEngineRegistrationBuilder instance, allowing for method chaining.</returns>
    public HypermediaEngineRegistrationBuilder WithLinkGenerationService<TLinkGenerationService>(
        Func<IServiceProvider, TLinkGenerationService> implementationFactory
    ) where TLinkGenerationService : class, ILinkGenerationService
    {
        if (IsLinkGenerationServiceRegistered) return this;
        Services = Services.AddScoped<ILinkGenerationService, TLinkGenerationService>(implementationFactory);
        IsLinkGenerationServiceRegistered = true;
        return this;
    }

    /// <summary>
    /// Registers the default link generation service if it has not already been registered.
    /// </summary>
    /// <remarks>This method ensures that an <see cref="ILinkGenerationService"/> implementation is available
    /// for dependency injection as a scoped service. If the service is already registered, this method has no
    /// effect.</remarks>
    /// <returns>The current instance of the <see cref="HypermediaEngineRegistrationBuilder"/> to support method chaining.</returns>
    public HypermediaEngineRegistrationBuilder WithDefaultLinkGenerationService()
    {
        if (IsLinkGenerationServiceRegistered) return this;
        Services = Services.AddScoped<ILinkGenerationService, LinkGenerationService>();
        IsLinkGenerationServiceRegistered = true;
        return this;
    }

    internal HypermediaEngineRegistrationBuilder WithResponseBuilders()
    {
        services
            .AddTransient(typeof(IHypermediaObjectBuilder<>), typeof(HypermediaObjectBuilder<>))
            .AddTransient(typeof(IHypermediaCollectionBuilder<>), typeof(HypermediaCollectionBuilder<>));
        return this;
    }

    internal HypermediaEngineRegistrationBuilder WithObjectResponseHandlers()
    {
        services
            .AddScoped(
                typeof(IResponseHandler),
                typeof(SimpleObjectResponseHandler))
            .AddScoped(
                typeof(AbstractObjectResponseHandler<>), 
                typeof(TResponseHandler<>));

        return WithObjectMetadataHandlers().WithObjectLinkHandlers();
    }

    internal HypermediaEngineRegistrationBuilder WithCollectionResponseHandlers()
    {
        services.AddScoped(
            typeof(AbstractCollectionResponseHandler<>),
            typeof(CollectionResponseHandler<>));
        return WithCollectionMetadataHandlers().WithCollectionLinkHandlers();
    }

    internal HypermediaEngineRegistrationBuilder WithResponseHandlersResolver()
    {
        services.AddScoped(
            typeof(IResponseHandlersResolver<>),
            typeof(ResponseHandlerResolver<>));
        return this;
    }

    /// <summary>
    /// Builds and configures the service collection for dependency injection.
    /// </summary>
    /// <remarks>This method registers the current instance as a scoped service, making it available for
    /// injection within the same scope. Use this method to finalize the service registration process before building
    /// the service provider.</remarks>
    /// <returns>The configured <see cref="IServiceCollection"/> instance that registers the current builder as a scoped service.</returns>
    internal IServiceCollection Build()
    {
        return Services.AddScoped(_ => this);
    }

    private HypermediaEngineRegistrationBuilder WithObjectMetadataHandlers()
    {
        services
            .AddScoped(
                typeof(AbstractObjectMetadataHandler<>),
                typeof(ObjectApiVersionMetadataHandler<>))
            .AddScoped(
                typeof(AbstractObjectMetadataHandler<>),
                typeof(ObjectEntityVersionMetadataHandler<>))
            .AddScoped(
                typeof(AbstractObjectMetadataHandler<>),
                typeof(ObjectEtagMetadataHandler<>));
        return this;
    }

    private HypermediaEngineRegistrationBuilder WithObjectLinkHandlers()
    {
        services
            .AddScoped(
                typeof(AbstractObjectLinkHandler<>),
                typeof(ObjectRelatedEndpointLinkHandler<>))
            .AddScoped(
                typeof(AbstractObjectLinkHandler<>),
                typeof(ObjectSelfEndpointLinkHandler<>))
            .AddScoped(
                typeof(AbstractObjectLinkHandler<>),
                typeof(ObjectStateTransitionEndpointLinkHandler<>));
        return this;
    }

    private HypermediaEngineRegistrationBuilder WithCollectionMetadataHandlers()
    {
        services
            .AddScoped(
                typeof(AbstractCollectionMetadataHandler<>),
                typeof(CollectionApiVersionMetadataHandler<>))
            .AddScoped(
                typeof(AbstractCollectionMetadataHandler<>),
                typeof(CollectionETagMetadataHandler<>))            ;
        return this;
    }

    private HypermediaEngineRegistrationBuilder WithCollectionLinkHandlers()
    {
        services
            .AddScoped(
                typeof(AbstractCollectionLinkHandler<>),
                typeof(CollectionRelatedEndpointLinkHandler<>))
            .AddScoped(
                typeof(AbstractCollectionLinkHandler<>),
                typeof(CollectionStateTransitionEndpointLinkHandler<>))
            .AddScoped(
                typeof(AbstractCollectionLinkHandler<>),
                typeof(CollectionPageEndpointLinkHandler<>));
        return this;
    }
}
