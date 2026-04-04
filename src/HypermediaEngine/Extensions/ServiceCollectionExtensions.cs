namespace HypermediaEngine.Extensions;

using HypermediaEngine.Interfaces;
using HypermediaEngine.Services;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHypermediaEngine(this IServiceCollection services)
    {
        services.AddScoped<IHypermediaService, HypermediaService>();
        services.AddScoped<IETagService, ETagService>();
        return services;
    }

    public static IServiceCollection AddHypermediaEngine(
        this IServiceCollection services,
        Action<HypermediaEngineOptions> configure)
    {
        services.Configure(configure);
        return services.AddHypermediaEngine();
    }
}
