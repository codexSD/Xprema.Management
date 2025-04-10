using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xprema.Framework.Sample;
using Xprema.Framework.Sample.Features;

namespace Xprema.Framework.tests.Sample;

public class SampleModuleTests
{
    private readonly IServiceCollection _services;
    private readonly IConfiguration _configuration;
    private readonly Mock<IApplicationBuilder> _appBuilderMock;
    private readonly SampleModule _module;
    private readonly Mock<ILoggingService> _loggingServiceMock;

    public SampleModuleTests()
    {
        _services = new ServiceCollection();
        _configuration = new ConfigurationBuilder().Build();
        _appBuilderMock = new Mock<IApplicationBuilder>();
        _module = new SampleModule();
        _loggingServiceMock = new Mock<ILoggingService>();
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        // Act
        var name = _module.Name;

        // Assert
        Assert.Equal("Sample Module", name);
    }

    [Fact]
    public void Description_ReturnsCorrectValue()
    {
        // Act
        var description = _module.Description;

        // Assert
        Assert.Equal("A sample module demonstrating the framework module concept", description);
    }

    [Fact]
    public void Version_ReturnsCorrectValue()
    {
        // Act
        var version = _module.Version;

        // Assert
        Assert.Equal("1.0.0", version);
    }

    [Fact]
    public void RegisterServices_RegistersSampleService()
    {
        // Act
        _module.RegisterServices(_services, _configuration);

        // Assert
        var serviceProvider = _services.BuildServiceProvider();
        var sampleService = serviceProvider.GetService<ISampleService>();

        Assert.NotNull(sampleService);
        Assert.IsType<SampleService>(sampleService);
    }

    [Fact]
    public void Configure_AddsSampleMiddleware()
    {
        // Arrange
        var middlewareAdded = false;
        
        _appBuilderMock.Setup(a => a.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()))
            .Callback<Func<RequestDelegate, RequestDelegate>>(_ => middlewareAdded = true)
            .Returns(_appBuilderMock.Object);

        // Act
        _module.Configure(_appBuilderMock.Object);

        // Assert
        _appBuilderMock.Verify(a => a.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()), Times.AtLeastOnce);
        Assert.True(middlewareAdded);
    }

    [Fact]
    public void SampleService_GetSampleData_ReturnsExpectedValue()
    {
        // Arrange
        var service = new SampleService(_loggingServiceMock.Object);

        // Act
        var result = service.GetSampleData();

        // Assert
        Assert.Equal("Sample data from the module", result);
        _loggingServiceMock.Verify(x => x.LogInfo("Getting sample data"), Times.Once);
    }

    [Fact]
    public void Module_HasLoggingFeature()
    {
        // Act
        var features = _module.Features;

        // Assert
        Assert.Contains(features, f => f is LoggingFeature);
    }
} 