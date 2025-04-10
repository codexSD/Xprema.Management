using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xprema.Framework.Entities.MultiTenancy;
using Xprema.Framework.Identity;
using Microsoft.AspNetCore.Identity;
using Xprema.Framework.Entities.Identity;

namespace Xprema.Framework.Identity;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly ITenantService _tenantService;
    private readonly ITenantContextAccessor _tenantContextAccessor;

    public AuthenticationService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService,
        ILogger<AuthenticationService> logger,
        ITenantService tenantService,
        ITenantContextAccessor tenantContextAccessor)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _logger = logger;
        _tenantService = tenantService;
        _tenantContextAccessor = tenantContextAccessor;
    }

    public async Task<AuthResult> RegisterUserAsync(RegisterUserRequest request)
    {
        try
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return new AuthResult
                {
                    Succeeded = false,
                    ErrorMessage = $"User with email '{request.Email}' already exists."
                };
            }

            existingUser = await _userManager.FindByNameAsync(request.Username);
            if (existingUser != null)
            {
                return new AuthResult
                {
                    Succeeded = false,
                    ErrorMessage = $"User with username '{request.Username}' already exists."
                };
            }

            var user = new ApplicationUser
            {
                UserName = request.Username,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                EmailConfirmed = false
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return new AuthResult
                {
                    Succeeded = false,
                    ErrorMessage = string.Join(", ", result.Errors.Select(e => e.Description))
                };
            }

            if (request.TenantId.HasValue && Guid.TryParse(user.Id, out var userGuid))
            {
                await _tenantService.AddUserToTenantAsync(request.TenantId.Value, userGuid, false, "system");
            }

            return new AuthResult
            {
                Succeeded = true,
                UserId = user.Id,
                Username = user.UserName,
                RequiresEmailConfirmation = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user: {Message}", ex.Message);
            return new AuthResult { Succeeded = false, ErrorMessage = "An error occurred during registration." };
        }
    }

    public async Task<AuthResult> AuthenticateAsync(LoginRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.UsernameOrEmail) ??
                  await _userManager.FindByEmailAsync(request.UsernameOrEmail);

        if (user == null)
        {
            return new AuthResult { Succeeded = false, ErrorMessage = "Invalid username or password." };
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, true);

        if (result.IsLockedOut)
        {
            return new AuthResult
            {
                Succeeded = false,
                IsLockedOut = true,
                ErrorMessage = "Account is locked out."
            };
        }

        if (!result.Succeeded)
        {
            return new AuthResult { Succeeded = false, ErrorMessage = "Invalid username or password." };
        }

        if (!user.EmailConfirmed)
        {
            return new AuthResult
            {
                Succeeded = false,
                RequiresEmailConfirmation = true,
                UserId = user.Id,
                ErrorMessage = "Email not confirmed."
            };
        }

        if (await _userManager.GetTwoFactorEnabledAsync(user))
        {
            if (!string.IsNullOrEmpty(request.TwoFactorCode))
            {
                var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, "Authenticator", request.TwoFactorCode);
                if (!isValid)
                {
                    return new AuthResult
                    {
                        Succeeded = false,
                        RequiresTwoFactor = true,
                        UserId = user.Id,
                        ErrorMessage = "Invalid two-factor code."
                    };
                }
            }
            else if (!string.IsNullOrEmpty(request.TwoFactorRecoveryCode))
            {
                var isValidRecovery = await _userManager.RedeemTwoFactorRecoveryCodeAsync(user, request.TwoFactorRecoveryCode);
                if (!isValidRecovery.Succeeded)
                {
                    return new AuthResult
                    {
                        Succeeded = false,
                        RequiresTwoFactor = true,
                        UserId = user.Id,
                        ErrorMessage = "Invalid recovery code."
                    };
                }
            }
            else
            {
                return new AuthResult
                {
                    Succeeded = false,
                    RequiresTwoFactor = true,
                    UserId = user.Id
                };
            }
        }

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = request.RememberMe ? _tokenService.GenerateRefreshToken() : null;

        return new AuthResult
        {
            Succeeded = true,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            UserId = user.Id,
            Username = user.UserName
        };
    }

    public async Task LogoutAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user != null)
        {
            await _signInManager.SignOutAsync();
        }
    }

    public async Task<bool> ConfirmEmailAsync(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        var result = await _userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded;
    }

    public async Task<bool> ForgotPasswordAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
        {
            // Return true to prevent email enumeration
            return true;
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        // TODO: Send password reset email
        return true;
    }

    public async Task<bool> ResetPasswordAsync(string userId, string token, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        return result.Succeeded;
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return false;
        }

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        return result.Succeeded;
    }

    public async Task<TwoFactorSetupResult> EnableTwoFactorAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return new TwoFactorSetupResult { Succeeded = false, ErrorMessage = "User not found." };
        }

        var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(unformattedKey))
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);
            unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        }

        var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

        return new TwoFactorSetupResult
        {
            Succeeded = true,
            SharedKey = unformattedKey,
            RecoveryCodes = recoveryCodes?.ToList()
        };
    }

    public async Task<bool> ValidateTwoFactorCodeAsync(Guid userId, string code)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return false;
        }

        return await _userManager.VerifyTwoFactorTokenAsync(user, "Authenticator", code);
    }

    public async Task<bool> DisableTwoFactorAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return false;
        }

        var result = await _userManager.SetTwoFactorEnabledAsync(user, false);
        return result.Succeeded;
    }

    public async Task<IEnumerable<string>> GetTwoFactorRecoveryCodesAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return Array.Empty<string>();
        }

        var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
        return recoveryCodes ?? Array.Empty<string>();
    }

    public async Task<TenantCreationResult> CreateTenantAsync(TenantCreationRequest request)
    {
        try
        {
            var tenant = await _tenantService.CreateTenantAsync(
                name: request.Name,
                identifier: request.Code,
                description: request.Description,
                storageMode: request.StorageMode != null ? Enum.Parse<TenantStorageMode>(request.StorageMode) : TenantStorageMode.SharedDatabase,
                connectionString: request.ConnectionString,
                createdBy: "system");

            if (request.AdminUserId.HasValue)
            {
                await _tenantService.AddUserToTenantAsync(tenant.Id, request.AdminUserId.Value, true, "system");
            }

            return new TenantCreationResult
            {
                Succeeded = true,
                TenantId = tenant.Id
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tenant: {Message}", ex.Message);
            return new TenantCreationResult { Succeeded = false, ErrorMessage = "An error occurred while creating the tenant." };
        }
    }

    public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
    {
        // TODO: Implement refresh token logic
        throw new NotImplementedException();
    }
} 