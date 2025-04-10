using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;

namespace Xprema.Framework.Core.Modules;

/// <summary>
/// Base class for framework modules providing default implementations
/// </summary>
public abstract class ModuleBase : IModule
{
    private readonly List<IFeature> _features = new();

    protected ModuleBase()
    {
        InitializeFeatures();
    }

    public abstract string Name { get; }
    public abstract string Description { get; }
    public virtual string Version => "1.0.0";
    public IEnumerable<IFeature> Features => _features.AsReadOnly();

    /// <summary>
    /// Initializes the features for this module. Override this method to add features.
    /// </summary>
    protected virtual void InitializeFeatures()
    {
        // Default implementation does nothing
    }

    /// <summary>
    /// Adds a feature to the module
    /// </summary>
    /// <param name="feature">The feature to add</param>
    protected void AddFeature(IFeature feature)
    {
        if (feature == null)
            throw new ArgumentNullException(nameof(feature));

        _features.Add(feature);
    }

    public virtual void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register all features
        foreach (var feature in Features)
        {
            feature.RegisterServices(services, configuration);
        }
    }

    public virtual void Configure(IApplicationBuilder app)
    {
        // Configure all features
        foreach (var feature in Features)
        {
            feature.Configure(app);
        }
    }
} 