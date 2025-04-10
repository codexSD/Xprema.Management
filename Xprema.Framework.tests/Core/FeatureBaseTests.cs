using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xprema.Framework.Core;

namespace Xprema.Framework.tests.Core;

public class FeatureBaseTests
{
    private class TestFeature : FeatureBase
    {
        public override string Name => "Test Feature";
        public override string Description => "Test Description";
    }

    [Fact]
    public void Version_ReturnsDefaultVersion()
    {
        // Arrange
        var feature = new TestFeature();

        // Act
        var version = feature.Version;

        // Assert
        Assert.Equal("1.0.0", version);
    }

    [Fact]
    public void RegisterServices_DoesNotThrowException()
    {
        // Arrange
        var feature = new TestFeature();
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act & Assert
        var exception = Record.Exception(() => feature.RegisterServices(services, configuration));
        Assert.Null(exception);
    }

    [Fact]
    public void Configure_DoesNotThrowException()
    {
        // Arrange
        var feature = new TestFeature();
        var appBuilder = new Mock<IApplicationBuilder>();

        // Act & Assert
        var exception = Record.Exception(() => feature.Configure(appBuilder.Object));
        Assert.Null(exception);
    }

    [Fact]
    public void Name_ReturnsOverriddenValue()
    {
        // Arrange
        var feature = new TestFeature();

        // Act
        var name = feature.Name;

        // Assert
        Assert.Equal("Test Feature", name);
    }

    [Fact]
    public void Description_ReturnsOverriddenValue()
    {
        // Arrange
        var feature = new TestFeature();

        // Act
        var description = feature.Description;

        // Assert
        Assert.Equal("Test Description", description);
    }
} 