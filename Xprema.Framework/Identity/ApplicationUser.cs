using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using Xprema.Framework.Entities.Common;
using Xprema.Framework.Entities.MultiTenancy;
using Xprema.Framework.Entities.Permission;
using Xprema.Framework.Entities.Identity;
using Xprema.Framework.Identity;

namespace Xprema.Framework.Identity;

/// <summary>
/// Represents a user in the application
/// </summary>
public class ApplicationUser : IdentityUser
{
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = null!;
    
    [Required]
    [MaxLength(100)]
    public override string Email { get; set; } = null!;
    
    [MaxLength(250)]
    public override string PasswordHash { get; set; }
    
    [MaxLength(256)]
    public string? DisplayName { get; set; }
    
    public override bool EmailConfirmed { get; set; }
    
    public override bool TwoFactorEnabled { get; set; }
    
    public DateTime? LastLoginDate { get; set; }
    
    [MaxLength(100)]
    public override string PhoneNumber { get; set; }
    
    public override bool PhoneNumberConfirmed { get; set; }
    
    public bool IsLockedOut { get; set; }
    
    public override DateTimeOffset? LockoutEnd { get; set; }
    
    public override int AccessFailedCount { get; set; }
    
    [MaxLength(36)]
    public override string SecurityStamp { get; set; }
    
    [MaxLength(100)]
    public string? TwoFactorKey { get; set; }
    
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string ProfilePicture { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; }
    public string TenantId { get; set; }
    public virtual Tenant Tenant { get; set; }
    
    // Navigation properties
    public virtual ICollection<IdentityUserRole<string>> UserRoles { get; set; } = new List<IdentityUserRole<string>>();
    public virtual ICollection<TenantUser> TenantUsers { get; set; } = new List<TenantUser>();
    public virtual ICollection<UserToken> UserTokens { get; set; } = new List<UserToken>();
} 