using Microsoft.AspNetCore.Identity;
using Xprema.Framework.Identity;
using Xprema.Framework.Entities.Common;
using Xprema.Framework.Entities.MultiTenancy;
using Xprema.Framework.Entities.Permission;
using System.ComponentModel.DataAnnotations;

namespace Xprema.Framework.Entities.Identity;

// Renamed to avoid conflict with Permission.UserRole
public class UserRoleIdentity : IdentityUserRole<string>, ITenantEntity
{
    public Guid TenantId { get; set; }
    
    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
    public virtual Tenant? Tenant { get; set; }
}

public class UserRole : IdentityUserRole<string>
{
    public virtual ApplicationUser User { get; set; }
    public virtual Role Role { get; set; }
} 