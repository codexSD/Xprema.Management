using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;

namespace Xprema.Framework.Core;

/// <summary>
/// Extension methods for registering framework modules
/// </summary>
public static class ModuleRegistrationExtensions
{
    /// <summary>
    /// Adds a framework module to the service collection
    /// </summary>
    /// <typeparam name="TModule">The type of module to add</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddFrameworkModule<TModule>(
        this IServiceCollection services,
        IConfiguration configuration) where TModule : class, IModule, new()
    {
        var module = new TModule();
        module.RegisterServices(services, configuration);
        services.AddSingleton(module);
        return services;
    }

    /// <summary>
    /// Configures all registered framework modules in the application pipeline
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder UseFrameworkModules(this IApplicationBuilder app)
    {
        var modules = app.ApplicationServices.GetServices<IModule>();
        foreach (var module in modules)
        {
            module.Configure(app);
        }
        return app;
    }
} 