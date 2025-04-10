using System.Security.Claims;
using Xprema.Framework.Identity;

namespace Xprema.Framework.Identity;

/// <summary>
/// Service for generating JWT tokens for authentication
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a JWT access token for a user
    /// </summary>
    string GenerateAccessToken(ApplicationUser user);
    
    /// <summary>
    /// Generates a refresh token
    /// </summary>
    string GenerateRefreshToken();
    
    /// <summary>
    /// Validates a JWT token
    /// </summary>
    bool ValidateToken(string token, out Dictionary<string, string> claims);

    Task<string> GenerateTokenAsync(ApplicationUser user);
    Task<ClaimsPrincipal> ValidateTokenAsync(string token);
} 