using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Xprema.Framework.Core;
using Xprema.Framework.Sample.Features;

namespace Xprema.Framework.Sample;

/// <summary>
/// Sample module demonstrating how to implement a framework module
/// </summary>
public class SampleModule : IModule
{
    private readonly IConfiguration _configuration;

    public SampleModule(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string Name => "Sample Module";
    public string Description => "A sample module demonstrating module functionality";
    public string Version => "1.0.0";
    public IEnumerable<IFeature> Features => Array.Empty<IFeature>();

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register sample services here
    }

    public void Configure(IApplicationBuilder app)
    {
        // Configure sample middleware here
    }
}

public interface ISampleService
{
    string GetSampleData();
}

public class SampleService : ISampleService
{
    private readonly ILoggingService _loggingService;

    public SampleService(ILoggingService loggingService)
    {
        _loggingService = loggingService;
    }

    public string GetSampleData()
    {
        _loggingService.LogInfo("Getting sample data");
        return "Sample data from the module";
    }
}

public class SampleMiddleware
{
    private readonly RequestDelegate _next;

    public SampleMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var loggingService = context.RequestServices.GetRequiredService<ILoggingService>();
        loggingService.LogInfo("Sample middleware executing");
        await _next.Invoke(context);
    }
} 