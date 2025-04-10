using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xprema.Framework.Identity;
using Xunit;

namespace Xprema.Framework.Tests.Identity;

public class IdentityModuleTests
{
    private readonly IConfiguration _configuration;
    private readonly IServiceCollection _services;
    private readonly IdentityModule _module;

    public IdentityModuleTests()
    {
        // Setup configuration
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(new Dictionary<string, string>
        {
            ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\mssqllocaldb;Database=XpremaTestDb;Trusted_Connection=True;MultipleActiveResultSets=true",
            ["Jwt:Key"] = "TestKey123456789012345678901234567890",
            ["Jwt:Issuer"] = "TestIssuer",
            ["Jwt:Audience"] = "TestAudience"
        });
        _configuration = configBuilder.Build();

        // Setup services
        _services = new ServiceCollection();
        _module = new IdentityModule(_configuration);
    }

    [Fact]
    public void ConfigureServices_RegistersRequiredServices()
    {
        // Act
        _module.ConfigureServices(_services);

        // Assert
        var serviceProvider = _services.BuildServiceProvider();

        // Verify DbContext registration
        var dbContext = serviceProvider.GetService<ApplicationDbContext>();
        Assert.NotNull(dbContext);

        // Verify Identity services
        var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();
        Assert.NotNull(userManager);

        var roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();
        Assert.NotNull(roleManager);

        // Verify JWT Authentication
        var authOptions = serviceProvider.GetService<JwtBearerOptions>();
        Assert.NotNull(authOptions);
    }

    [Fact]
    public void ConfigureServices_SetsCorrectIdentityOptions()
    {
        // Act
        _module.ConfigureServices(_services);

        // Assert
        var serviceProvider = _services.BuildServiceProvider();
        var identityOptions = serviceProvider.GetService<IdentityOptions>();

        Assert.NotNull(identityOptions);
        Assert.True(identityOptions.Password.RequireDigit);
        Assert.True(identityOptions.Password.RequireLowercase);
        Assert.True(identityOptions.Password.RequireNonAlphanumeric);
        Assert.True(identityOptions.Password.RequireUppercase);
        Assert.Equal(8, identityOptions.Password.RequiredLength);
        Assert.Equal(TimeSpan.FromMinutes(30), identityOptions.Lockout.DefaultLockoutTimeSpan);
        Assert.Equal(5, identityOptions.Lockout.MaxFailedAccessAttempts);
        Assert.True(identityOptions.Lockout.AllowedForNewUsers);
        Assert.True(identityOptions.User.RequireUniqueEmail);
    }

    [Fact]
    public void ConfigureServices_SetsCorrectJwtOptions()
    {
        // Act
        _module.ConfigureServices(_services);

        // Assert
        var serviceProvider = _services.BuildServiceProvider();
        var authOptions = serviceProvider.GetService<JwtBearerOptions>();

        Assert.NotNull(authOptions);
        Assert.NotNull(authOptions.TokenValidationParameters);
        Assert.True(authOptions.TokenValidationParameters.ValidateIssuer);
        Assert.True(authOptions.TokenValidationParameters.ValidateAudience);
        Assert.True(authOptions.TokenValidationParameters.ValidateLifetime);
        Assert.True(authOptions.TokenValidationParameters.ValidateIssuerSigningKey);
        Assert.Equal("TestIssuer", authOptions.TokenValidationParameters.ValidIssuer);
        Assert.Equal("TestAudience", authOptions.TokenValidationParameters.ValidAudience);
    }
} 
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xprema.Framework.Identity;
using Xunit;

namespace Xprema.Framework.Tests.Identity;

public class IdentityModuleTests
{
    private readonly IConfiguration _configuration;
    private readonly IServiceCollection _services;
    private readonly IdentityModule _module;

    public IdentityModuleTests()
    {
        // Setup configuration
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(new Dictionary<string, string>
        {
            ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\mssqllocaldb;Database=XpremaTestDb;Trusted_Connection=True;MultipleActiveResultSets=true",
            ["Jwt:Key"] = "TestKey123456789012345678901234567890",
            ["Jwt:Issuer"] = "TestIssuer",
            ["Jwt:Audience"] = "TestAudience"
        });
        _configuration = configBuilder.Build();

        // Setup services
        _services = new ServiceCollection();
        _module = new IdentityModule(_configuration);
    }

    [Fact]
    public void ConfigureServices_RegistersRequiredServices()
    {
        // Act
        _module.ConfigureServices(_services);

        // Assert
        var serviceProvider = _services.BuildServiceProvider();

        // Verify DbContext registration
        var dbContext = serviceProvider.GetService<ApplicationDbContext>();
        Assert.NotNull(dbContext);

        // Verify Identity services
        var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();
        Assert.NotNull(userManager);

        var roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();
        Assert.NotNull(roleManager);

        // Verify JWT Authentication
        var authOptions = serviceProvider.GetService<JwtBearerOptions>();
        Assert.NotNull(authOptions);
    }

    [Fact]
    public void ConfigureServices_SetsCorrectIdentityOptions()
    {
        // Act
        _module.ConfigureServices(_services);

        // Assert
        var serviceProvider = _services.BuildServiceProvider();
        var identityOptions = serviceProvider.GetService<IdentityOptions>();

        Assert.NotNull(identityOptions);
        Assert.True(identityOptions.Password.RequireDigit);
        Assert.True(identityOptions.Password.RequireLowercase);
        Assert.True(identityOptions.Password.RequireNonAlphanumeric);
        Assert.True(identityOptions.Password.RequireUppercase);
        Assert.Equal(8, identityOptions.Password.RequiredLength);
        Assert.Equal(TimeSpan.FromMinutes(30), identityOptions.Lockout.DefaultLockoutTimeSpan);
        Assert.Equal(5, identityOptions.Lockout.MaxFailedAccessAttempts);
        Assert.True(identityOptions.Lockout.AllowedForNewUsers);
        Assert.True(identityOptions.User.RequireUniqueEmail);
    }

    [Fact]
    public void ConfigureServices_SetsCorrectJwtOptions()
    {
        // Act
        _module.ConfigureServices(_services);

        // Assert
        var serviceProvider = _services.BuildServiceProvider();
        var authOptions = serviceProvider.GetService<JwtBearerOptions>();

        Assert.NotNull(authOptions);
        Assert.NotNull(authOptions.TokenValidationParameters);
        Assert.True(authOptions.TokenValidationParameters.ValidateIssuer);
        Assert.True(authOptions.TokenValidationParameters.ValidateAudience);
        Assert.True(authOptions.TokenValidationParameters.ValidateLifetime);
        Assert.True(authOptions.TokenValidationParameters.ValidateIssuerSigningKey);
        Assert.Equal("TestIssuer", authOptions.TokenValidationParameters.ValidIssuer);
        Assert.Equal("TestAudience", authOptions.TokenValidationParameters.ValidAudience);
    }
} 