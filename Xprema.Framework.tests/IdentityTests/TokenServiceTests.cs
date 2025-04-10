using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xprema.Framework.Identity;
using Xprema.Framework.Entities.Identity;
using Xunit;

namespace Xprema.Framework.tests.IdentityTests
{
    public class TokenServiceTests : TestBase
    {
        private readonly TokenService _tokenService;
        private readonly IConfiguration _configuration;

        public TokenServiceTests()
        {
            _configuration = ServiceProvider.GetRequiredService<IConfiguration>();
            _tokenService = new TokenService(_configuration);
        }

        [Fact]
        public async Task GenerateAccessToken_ShouldReturnValidToken()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "test@example.com",
                Email = "test@example.com"
            };

            // Act
            var token = _tokenService.GenerateAccessToken(user);

            // Assert
            Assert.NotNull(token);
            Assert.NotEmpty(token);
        }

        [Fact]
        public async Task ValidateToken_ShouldReturnTrueForValidToken()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "test@example.com",
                Email = "test@example.com"
            };
            var token = _tokenService.GenerateAccessToken(user);

            // Act
            var isValid = _tokenService.ValidateToken(token, out var claims);

            // Assert
            Assert.True(isValid);
            Assert.NotNull(claims);
            Assert.Contains(claims, c => c.Key == "sub" && c.Value == user.Id);
        }

        [Fact]
        public async Task GenerateRefreshToken_ShouldReturnUniqueTokens()
        {
            // Act
            var token1 = _tokenService.GenerateRefreshToken();
            var token2 = _tokenService.GenerateRefreshToken();

            // Assert
            Assert.NotNull(token1);
            Assert.NotNull(token2);
            Assert.NotEqual(token1, token2);
        }

        [Fact]
        public async Task ValidateToken_ShouldReturnFalseForInvalidToken()
        {
            // Arrange
            var invalidToken = "invalid.token.here";

            // Act
            var isValid = _tokenService.ValidateToken(invalidToken, out var claims);

            // Assert
            Assert.False(isValid);
            Assert.Null(claims);
        }
    }
} 