using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xprema.Framework.Core;
using Xprema.Framework.Sample;

namespace Xprema.Framework.tests.Core;

public class ModuleRegistrationExtensionsTests
{
    private readonly IServiceCollection _services;
    private readonly IConfiguration _configuration;
    private readonly Mock<IApplicationBuilder> _appBuilderMock;

    public ModuleRegistrationExtensionsTests()
    {
        _services = new ServiceCollection();
        _configuration = new ConfigurationBuilder().Build();
        _appBuilderMock = new Mock<IApplicationBuilder>();
    }

    [Fact]
    public void AddFrameworkModule_RegistersModuleAsSingleton()
    {
        // Act
        _services.AddFrameworkModule<SampleModule>(_configuration);

        // Assert
        var serviceProvider = _services.BuildServiceProvider();
        var module1 = serviceProvider.GetService<SampleModule>();
        var module2 = serviceProvider.GetService<SampleModule>();

        Assert.NotNull(module1);
        Assert.Same(module1, module2); // Should be the same instance (singleton)
    }

    [Fact]
    public void AddFrameworkModule_RegistersServices()
    {
        // Act
        _services.AddFrameworkModule<SampleModule>(_configuration);

        // Assert
        var serviceProvider = _services.BuildServiceProvider();
        var sampleService = serviceProvider.GetService<ISampleService>();

        Assert.NotNull(sampleService);
        Assert.IsType<SampleService>(sampleService);
    }

    [Fact]
    public void UseFrameworkModules_ConfiguresAllRegisteredModules()
    {
        // Arrange
        var configureCalled = false;
        var mockModule = new Mock<IModule>();
        mockModule.Setup(m => m.Configure(It.IsAny<IApplicationBuilder>()))
            .Callback(() => configureCalled = true);

        _services.AddSingleton(mockModule.Object);
        var serviceProvider = _services.BuildServiceProvider();
        _appBuilderMock.Setup(a => a.ApplicationServices).Returns(serviceProvider);

        // Act
        _appBuilderMock.Object.UseFrameworkModules();

        // Assert
        mockModule.Verify(m => m.Configure(It.IsAny<IApplicationBuilder>()), Times.Once);
        Assert.True(configureCalled);
    }

    [Fact]
    public void UseFrameworkModules_ConfiguresMultipleModules()
    {
        // Arrange
        var module1ConfigureCalled = false;
        var module2ConfigureCalled = false;
        var mockModule1 = new Mock<IModule>();
        var mockModule2 = new Mock<IModule>();

        mockModule1.Setup(m => m.Configure(It.IsAny<IApplicationBuilder>()))
            .Callback(() => module1ConfigureCalled = true);
        mockModule2.Setup(m => m.Configure(It.IsAny<IApplicationBuilder>()))
            .Callback(() => module2ConfigureCalled = true);

        _services.AddSingleton(mockModule1.Object);
        _services.AddSingleton(mockModule2.Object);
        var serviceProvider = _services.BuildServiceProvider();
        _appBuilderMock.Setup(a => a.ApplicationServices).Returns(serviceProvider);

        // Act
        _appBuilderMock.Object.UseFrameworkModules();

        // Assert
        Assert.True(module1ConfigureCalled);
        Assert.True(module2ConfigureCalled);
        mockModule1.Verify(m => m.Configure(It.IsAny<IApplicationBuilder>()), Times.Once);
        mockModule2.Verify(m => m.Configure(It.IsAny<IApplicationBuilder>()), Times.Once);
    }
} 