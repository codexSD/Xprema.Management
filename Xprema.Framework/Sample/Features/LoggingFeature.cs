using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xprema.Framework.Core;

namespace Xprema.Framework.Sample.Features;

public class LoggingFeature : FeatureBase
{
    public override string Name => "Logging Feature";
    public override string Description => "Provides logging capabilities for the module";

    public override void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddLogging(builder =>
        {
            builder.AddConfiguration(configuration.GetSection("Logging"));
            builder.AddConsole();
            builder.AddDebug();
        });

        services.AddScoped<ILoggingService, LoggingService>();
    }

    public override void Configure(IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<LoggingFeature>>();
            logger.LogInformation($"Request: {context.Request.Path}");
            await next();
            logger.LogInformation($"Response: {context.Response.StatusCode}");
        });
    }
}

public interface ILoggingService
{
    void LogInfo(string message);
    void LogError(string message, Exception? exception = null);
}

public class LoggingService : ILoggingService
{
    private readonly ILogger<LoggingService> _logger;

    public LoggingService(ILogger<LoggingService> logger)
    {
        _logger = logger;
    }

    public void LogInfo(string message)
    {
        _logger.LogInformation(message);
    }

    public void LogError(string message, Exception? exception = null)
    {
        _logger.LogError(exception, message);
    }
} 