using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Xprema.Framework.Entities.Common;
using Xprema.Framework.Entities.HistoryFeature;
using Xprema.Framework.Entities.Identity;
using Xprema.Framework.Entities.MultiTenancy;
using Xprema.Framework.Entities.Permission;
using Xprema.Framework.Identity;

namespace Xprema.Framework.tests;

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }
    
    // Permission entities
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<Permission> Permissions { get; set; } = null!;
    public DbSet<RolePermission> RolePermissions { get; set; } = null!;
    public DbSet<Xprema.Framework.Entities.Permission.UserRole> UserRoles { get; set; } = null!;
    
    // Tenant entities
    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<TenantUser> TenantUsers { get; set; } = null!;
    
    // Identity entities
    public DbSet<ApplicationUser> Users { get; set; } = null!;
    public DbSet<UserToken> UserTokens { get; set; } = null!;
    public DbSet<UserRoleIdentity> UserRoleIdentities { get; set; } = null!;
    
    // Audit entities
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<EntityHistoryRecord> EntityHistoryRecords { get; set; } = null!;
    public DbSet<EntityPropertyChangeRecord> EntityPropertyChangeRecords { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure entity relationships for permission entities
        modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Role)
            .WithMany()
            .HasForeignKey(rp => rp.RoleId);
            
        modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Permission)
            .WithMany()
            .HasForeignKey(rp => rp.PermissionId);
            
        modelBuilder.Entity<Xprema.Framework.Entities.Permission.UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany()
            .HasForeignKey(ur => ur.RoleId);
            
        // Configure entity relationships for tenant entities
        modelBuilder.Entity<TenantUser>()
            .HasOne(tu => tu.Tenant)
            .WithMany()
            .HasForeignKey(tu => tu.TenantId);
            
        // Configure entity relationships for identity entities
        modelBuilder.Entity<UserToken>()
            .HasOne(ut => ut.User)
            .WithMany()
            .HasForeignKey(ut => ut.UserId);
            
        modelBuilder.Entity<UserRoleIdentity>()
            .HasOne(uri => uri.User)
            .WithMany()
            .HasForeignKey(uri => uri.UserId);
            
        modelBuilder.Entity<UserRoleIdentity>()
            .HasOne(uri => uri.Role)
            .WithMany()
            .HasForeignKey(uri => uri.RoleId);
            
        // Configure entity relationships for audit entities
        modelBuilder.Entity<EntityPropertyChangeRecord>()
            .HasOne(epcr => epcr.EntityHistoryRecord)
            .WithMany()
            .HasForeignKey(epcr => epcr.EntityHistoryRecordId);
    }
} 