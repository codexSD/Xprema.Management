using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xprema.Framework.Entities.Identity;
using Xprema.Framework.Entities.MultiTenancy;
using Xunit;

namespace Xprema.Framework.Tests.Identity;

public class AuthenticationServiceTests
{
    private readonly Mock<DbContext> _dbContextMock;
    private readonly Mock<ILogger<AuthenticationService>> _loggerMock;
    private readonly Mock<ITenantService> _tenantServiceMock;
    private readonly Mock<ITenantContextAccessor> _tenantContextAccessorMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly AuthenticationService _authService;

    public AuthenticationServiceTests()
    {
        _dbContextMock = new Mock<DbContext>();
        _loggerMock = new Mock<ILogger<AuthenticationService>>();
        _tenantServiceMock = new Mock<ITenantService>();
        _tenantContextAccessorMock = new Mock<ITenantContextAccessor>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _tokenServiceMock = new Mock<ITokenService>();

        _authService = new AuthenticationService(
            _dbContextMock.Object,
            _loggerMock.Object,
            _tenantServiceMock.Object,
            _tenantContextAccessorMock.Object,
            _httpContextAccessorMock.Object,
            _tokenServiceMock.Object
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

        var dbSetMock = new Mock<DbSet<ApplicationUser>>();
        _dbContextMock.Setup(x => x.Set<ApplicationUser>())
            .Returns(dbSetMock.Object);

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
            Username = "existinguser",
            Email = "existing@example.com"
        };

        var users = new List<ApplicationUser> { existingUser }.AsQueryable();
        var dbSetMock = new Mock<DbSet<ApplicationUser>>();
        dbSetMock.As<IQueryable<ApplicationUser>>().Setup(m => m.Provider).Returns(users.Provider);
        dbSetMock.As<IQueryable<ApplicationUser>>().Setup(m => m.Expression).Returns(users.Expression);
        dbSetMock.As<IQueryable<ApplicationUser>>().Setup(m => m.ElementType).Returns(users.ElementType);
        dbSetMock.As<IQueryable<ApplicationUser>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

        _dbContextMock.Setup(x => x.Set<ApplicationUser>())
            .Returns(dbSetMock.Object);

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
            Username = "testuser",
            Email = "test@example.com",
            EmailConfirmed = true,
            PasswordHash = "hashedpassword" // In real scenario, this would be properly hashed
        };

        var users = new List<ApplicationUser> { user }.AsQueryable();
        var dbSetMock = new Mock<DbSet<ApplicationUser>>();
        dbSetMock.As<IQueryable<ApplicationUser>>().Setup(m => m.Provider).Returns(users.Provider);
        dbSetMock.As<IQueryable<ApplicationUser>>().Setup(m => m.Expression).Returns(users.Expression);
        dbSetMock.As<IQueryable<ApplicationUser>>().Setup(m => m.ElementType).Returns(users.ElementType);
        dbSetMock.As<IQueryable<ApplicationUser>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

        _dbContextMock.Setup(x => x.Set<ApplicationUser>())
            .Returns(dbSetMock.Object);

        _tokenServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<ApplicationUser>()))
            .Returns("test_access_token");
        _tokenServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("test_refresh_token");

        // Act
        var result = await _authService.AuthenticateAsync(request);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.AccessToken);
        Assert.NotNull(result.RefreshToken);
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
            Username = "testuser",
            Email = "test@example.com",
            EmailConfirmed = true,
            PasswordHash = "hashedpassword"
        };

        var users = new List<ApplicationUser> { user }.AsQueryable();
        var dbSetMock = new Mock<DbSet<ApplicationUser>>();
        dbSetMock.As<IQueryable<ApplicationUser>>().Setup(m => m.Provider).Returns(users.Provider);
        dbSetMock.As<IQueryable<ApplicationUser>>().Setup(m => m.Expression).Returns(users.Expression);
        dbSetMock.As<IQueryable<ApplicationUser>>().Setup(m => m.ElementType).Returns(users.ElementType);
        dbSetMock.As<IQueryable<ApplicationUser>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

        _dbContextMock.Setup(x => x.Set<ApplicationUser>())
            .Returns(dbSetMock.Object);

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
            Username = "testuser"
        };

        var userToken = new UserToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.Parse(user.Id),
            TokenType = "RefreshToken",
            TokenValue = refreshToken,
            ExpirationDate = DateTime.UtcNow.AddDays(1),
            IsUsed = false,
            User = user
        };

        var tokens = new List<UserToken> { userToken }.AsQueryable();
        var dbSetMock = new Mock<DbSet<UserToken>>();
        dbSetMock.As<IQueryable<UserToken>>().Setup(m => m.Provider).Returns(tokens.Provider);
        dbSetMock.As<IQueryable<UserToken>>().Setup(m => m.Expression).Returns(tokens.Expression);
        dbSetMock.As<IQueryable<UserToken>>().Setup(m => m.ElementType).Returns(tokens.ElementType);
        dbSetMock.As<IQueryable<UserToken>>().Setup(m => m.GetEnumerator()).Returns(tokens.GetEnumerator());

        _dbContextMock.Setup(x => x.Set<UserToken>())
            .Returns(dbSetMock.Object);

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
        var tokens = new List<UserToken>().AsQueryable();
        var dbSetMock = new Mock<DbSet<UserToken>>();
        dbSetMock.As<IQueryable<UserToken>>().Setup(m => m.Provider).Returns(tokens.Provider);
        dbSetMock.As<IQueryable<UserToken>>().Setup(m => m.Expression).Returns(tokens.Expression);
        dbSetMock.As<IQueryable<UserToken>>().Setup(m => m.ElementType).Returns(tokens.ElementType);
        dbSetMock.As<IQueryable<UserToken>>().Setup(m => m.GetEnumerator()).Returns(tokens.GetEnumerator());

        _dbContextMock.Setup(x => x.Set<UserToken>())
            .Returns(dbSetMock.Object);

        // Act
        var result = await _authService.RefreshTokenAsync(refreshToken);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Invalid or expired refresh token", result.ErrorMessage);
    }
} 