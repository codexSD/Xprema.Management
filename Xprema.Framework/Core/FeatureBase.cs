using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Xprema.Framework.Core;

/// <summary>
/// Base class for features providing default implementations
/// </summary>
public abstract class FeatureBase : IFeature
{
    public abstract string Name { get; }
    public abstract string Description { get; }
    public virtual string Version => "1.0.0";

    public virtual void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // Default implementation does nothing
    }

    public virtual void Configure(IApplicationBuilder app)
    {
        // Default implementation does nothing
    }
} 