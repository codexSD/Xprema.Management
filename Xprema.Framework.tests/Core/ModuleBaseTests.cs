using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xprema.Framework.Core;
using Xprema.Framework.Core.Modules;

namespace Xprema.Framework.Tests.Core;

public class ModuleBaseTests
{
    private class TestModule : ModuleBase
    {
        public override string Name => "Test Module";
        public override string Description => "Test Description";
    }

    [Fact]
    public void Version_ReturnsDefaultVersion()
    {
        // Arrange
        var module = new TestModule();

        // Act
        var version = module.Version;

        // Assert
        Assert.Equal("1.0.0", version);
    }

    [Fact]
    public void RegisterServices_DoesNotThrowException()
    {
        // Arrange
        var module = new TestModule();
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act & Assert
        var exception = Record.Exception(() => module.RegisterServices(services, configuration));
        Assert.Null(exception);
    }

    [Fact]
    public void Configure_DoesNotThrowException()
    {
        // Arrange
        var module = new TestModule();
        var appBuilder = new Mock<IApplicationBuilder>();

        // Act & Assert
        var exception = Record.Exception(() => module.Configure(appBuilder.Object));
        Assert.Null(exception);
    }

    [Fact]
    public void Name_ReturnsOverriddenValue()
    {
        // Arrange
        var module = new TestModule();

        // Act
        var name = module.Name;

        // Assert
        Assert.Equal("Test Module", name);
    }

    [Fact]
    public void Description_ReturnsOverriddenValue()
    {
        // Arrange
        var module = new TestModule();

        // Act
        var description = module.Description;

        // Assert
        Assert.Equal("Test Description", description);
    }
} 