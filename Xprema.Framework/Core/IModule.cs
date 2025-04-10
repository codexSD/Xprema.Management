using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using System.Collections.Generic;

namespace Xprema.Framework.Core;

/// <summary>
/// Represents a framework module that can be registered in an ASP.NET application
/// </summary>
public interface IModule
{
    /// <summary>
    /// Gets the name of the module
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the description of the module
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the version of the module
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Gets the features registered in this module
    /// </summary>
    IEnumerable<IFeature> Features { get; }

    /// <summary>
    /// Registers the module's services with the dependency injection container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    void RegisterServices(IServiceCollection services, IConfiguration configuration);

    /// <summary>
    /// Configures the module's middleware pipeline
    /// </summary>
    /// <param name="app">The application builder</param>
    void Configure(IApplicationBuilder app);
} 