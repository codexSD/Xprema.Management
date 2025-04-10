using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Xprema.Framework.Core;

/// <summary>
/// Represents a feature that can be injected into a module
/// </summary>
public interface IFeature
{
    /// <summary>
    /// Gets the name of the feature
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the description of the feature
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the version of the feature
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Registers the feature's services with the dependency injection container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    void RegisterServices(IServiceCollection services, IConfiguration configuration);

    /// <summary>
    /// Configures the feature's middleware pipeline
    /// </summary>
    /// <param name="app">The application builder</param>
    void Configure(IApplicationBuilder app);
} 