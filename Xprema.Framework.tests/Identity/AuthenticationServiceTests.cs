using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xprema.Framework.Identity;
using Xprema.Framework.Entities.Identity;
using Xprema.Framework.Entities.MultiTenancy;
using Xunit;
using Microsoft.AspNetCore.Authentication;
using IAuthenticationService = Xprema.Framework.Entities.Identity.IAuthenticationService;
using AuthenticationService = Xprema.Framework.Identity.AuthenticationService;

namespace Xprema.Framework.Tests.Identity;

public class AuthenticationServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<ILogger<AuthenticationService>> _loggerMock;
    private readonly Mock<ITenantService> _tenantServiceMock;
    private readonly Mock<ITenantContextAccessor> _tenantContextAccessorMock;
    private readonly IAuthenticationService _authService;

    public AuthenticationServiceTests()
    {
        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        var userValidators = new List<IUserValidator<ApplicationUser>>();
        var passwordValidators = new List<IPasswordValidator<ApplicationUser>>();
        var lookupNormalizer = new Mock<ILookupNormalizer>();
        var errorDescriber = new Mock<IdentityErrorDescriber>();
        var services = new Mock<IServiceProvider>();
        var logger = new Mock<ILogger<UserManager<ApplicationUser>>>();
        var options = new Mock<IOptions<IdentityOptions>>();
        var passwordHasher = new Mock<IPasswordHasher<ApplicationUser>>();

        options.Setup(o => o.Value).Returns(new IdentityOptions());

        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object,
            options.Object,
            passwordHasher.Object,
            userValidators,
            passwordValidators,
            lookupNormalizer.Object,
            errorDescriber.Object,
            services.Object,
            logger.Object);
            
        var contextAccessorMock = new Mock<IHttpContextAccessor>();
        var claimsFactoryMock = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
        var signInLogger = new Mock<ILogger<SignInManager<ApplicationUser>>>();
        var schemeProvider = new Mock<IAuthenticationSchemeProvider>();
        var userConfirmation = new Mock<IUserConfirmation<ApplicationUser>>();
        var signInOptions = new Mock<IOptions<IdentityOptions>>();

        signInOptions.Setup(o => o.Value).Returns(new IdentityOptions());

        _signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
            _userManagerMock.Object,
            contextAccessorMock.Object,
            claimsFactoryMock.Object,
            signInOptions.Object,
            signInLogger.Object,
            schemeProvider.Object,
            userConfirmation.Object);
            
        _tokenServiceMock = new Mock<ITokenService>();
        _loggerMock = new Mock<ILogger<AuthenticationService>>();
        _tenantServiceMock = new Mock<ITenantService>();
        _tenantContextAccessorMock = new Mock<ITenantContextAccessor>();

        _authService = new AuthenticationService(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _tokenServiceMock.Object,
            _loggerMock.Object,
            _tenantServiceMock.Object,
            _tenantContextAccessorMock.Object
        );
    }

    [Fact]
    public async Task RegisterUserAsync_WithValidRequest_ReturnsSuccessResult()
    {
        // Arrange
        var request = new RegisterUserRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Test123!@#",
            DisplayName = "Test User",
            PhoneNumber = "1234567890"
        };

        _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
            .ReturnsAsync((ApplicationUser?)null);
            
        _userManagerMock.Setup(x => x.FindByNameAsync(request.Username))
            .ReturnsAsync((ApplicationUser?)null);
            
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _authService.RegisterUserAsync(request);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.UserId);
        Assert.Equal(request.Username, result.Username);
        Assert.True(result.RequiresEmailConfirmation);
    }

    [Fact]
    public async Task RegisterUserAsync_WithExistingUsername_ReturnsFailureResult()
    {
        // Arrange
        var request = new RegisterUserRequest
        {
            Username = "existinguser",
            Email = "test@example.com",
            Password = "Test123!@#"
        };

        var existingUser = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "existinguser",
            Email = "existing@example.com"
        };

        _userManagerMock.Setup(x => x.FindByNameAsync(request.Username))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _authService.RegisterUserAsync(request);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("already exists", result.ErrorMessage);
    }

    [Fact]
    public async Task AuthenticateAsync_WithValidCredentials_ReturnsSuccessResult()
    {
        // Arrange
        var request = new LoginRequest
        {
            UsernameOrEmail = "testuser",
            Password = "Test123!@#"
        };

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "testuser",
            Email = "test@example.com",
            EmailConfirmed = true
        };

        _userManagerMock.Setup(x => x.FindByNameAsync(request.UsernameOrEmail))
            .ReturnsAsync(user);
            
        _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, request.Password, true))
            .ReturnsAsync(SignInResult.Success);
            
        _userManagerMock.Setup(x => x.GetTwoFactorEnabledAsync(user))
            .ReturnsAsync(false);

        _tokenServiceMock.Setup(x => x.GenerateAccessToken(user))
            .Returns("test_access_token");
        _tokenServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("test_refresh_token");

        // Act
        var result = await _authService.AuthenticateAsync(request);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.AccessToken);
        Assert.Equal("test_access_token", result.AccessToken);
        Assert.NotNull(result.ExpiresAt);
    }

    [Fact]
    public async Task AuthenticateAsync_WithInvalidCredentials_ReturnsFailureResult()
    {
        // Arrange
        var request = new LoginRequest
        {
            UsernameOrEmail = "testuser",
            Password = "wrongpassword"
        };

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "testuser",
            Email = "test@example.com",
            EmailConfirmed = true
        };

        _userManagerMock.Setup(x => x.FindByNameAsync(request.UsernameOrEmail))
            .ReturnsAsync(user);
            
        _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, request.Password, true))
            .ReturnsAsync(SignInResult.Failed);

        // Act
        var result = await _authService.AuthenticateAsync(request);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Invalid username or password", result.ErrorMessage);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ReturnsNewTokens()
    {
        // Arrange
        var refreshToken = "valid_refresh_token";
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "testuser",
            Email = "test@example.com"
        };

        _tokenServiceMock.Setup(x => x.ValidateRefreshTokenAsync(refreshToken))
            .ReturnsAsync((true, user.Id));
        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id))
            .ReturnsAsync(user);
        _tokenServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<ApplicationUser>()))
            .Returns("new_access_token");
        _tokenServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("new_refresh_token");

        // Act
        var result = await _authService.RefreshTokenAsync(refreshToken);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("new_access_token", result.AccessToken);
        Assert.Equal("new_refresh_token", result.RefreshToken);
        Assert.NotNull(result.ExpiresAt);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithInvalidToken_ReturnsFailureResult()
    {
        // Arrange
        var refreshToken = "invalid_refresh_token";
        _tokenServiceMock.Setup(x => x.ValidateRefreshTokenAsync(refreshToken))
            .ReturnsAsync((false, string.Empty));

        // Act
        var result = await _authService.RefreshTokenAsync(refreshToken);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Invalid or expired refresh token", result.ErrorMessage);
    }
} 