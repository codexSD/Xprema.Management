using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xprema.Framework.Core;
using Xprema.Framework.Core.Modules;

namespace Xprema.Framework.Tests.Core;

public class ModuleFeatureTests
{
    private class TestFeature : FeatureBase
    {
        public bool RegisterServicesCalled { get; private set; }
        public bool ConfigureCalled { get; private set; }

        public override string Name => "Test Feature";
        public override string Description => "Test Description";

        public override void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            RegisterServicesCalled = true;
        }

        public override void Configure(IApplicationBuilder app)
        {
            ConfigureCalled = true;
        }
    }

    private class TestModule : ModuleBase
    {
        private TestFeature? _feature;

        public TestModule(TestFeature? feature = null)
        {
            if (feature != null)
            {
                SetFeature(feature);
            }
        }

        public override string Name => "Test Module";
        public override string Description => "Test Description";

        public void SetFeature(TestFeature feature)
        {
            if (feature == null)
            {
                throw new ArgumentNullException(nameof(feature));
            }

            _feature = feature;
            InitializeFeatures();
        }

        protected override void InitializeFeatures()
        {
            base.InitializeFeatures();
            if (_feature != null)
            {
                AddFeature(_feature);
            }
        }
    }

    [Fact]
    public void AddFeature_AddsFeatureToCollection()
    {
        // Arrange
        var feature = new TestFeature();
        var module = new TestModule(feature);

        // Act
        var features = module.Features;

        // Assert
        Assert.Single(features);
        Assert.Same(feature, features.First());
    }

    [Fact]
    public void RegisterServices_CallsFeatureRegisterServices()
    {
        // Arrange
        var feature = new TestFeature();
        var module = new TestModule(feature);
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act
        module.RegisterServices(services, configuration);

        // Assert
        Assert.True(feature.RegisterServicesCalled);
    }

    [Fact]
    public void Configure_CallsFeatureConfigure()
    {
        // Arrange
        var feature = new TestFeature();
        var module = new TestModule(feature);
        var appBuilder = new Mock<IApplicationBuilder>();

        // Act
        module.Configure(appBuilder.Object);

        // Assert
        Assert.True(feature.ConfigureCalled);
    }

    [Fact]
    public void AddFeature_WithNullFeature_ThrowsArgumentNullException()
    {
        // Arrange
        var module = new TestModule();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => module.SetFeature(null!));
    }
} 