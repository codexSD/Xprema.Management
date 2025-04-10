using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xprema.Framework.Entities.MultiTenancy;
using Xprema.Framework.Entities.Permission;
using Xprema.Framework.tests;
using Xprema.Framework.Identity;
using Xunit;

namespace Xprema.Framework.Tests;

public class PermissionServiceTests : TestBase, IAsyncLifetime
{
    private readonly TenantContextAccessor<TestDbContext> _tenantContextAccessor;
    private readonly PermissionService<TestDbContext> _permissionService;

    public PermissionServiceTests()
    {
        _tenantContextAccessor = ServiceProvider.GetRequiredService<TenantContextAccessor<TestDbContext>>();
        _permissionService = ServiceProvider.GetRequiredService<PermissionService<TestDbContext>>();
    }

    public async Task InitializeAsync()
    {
        // Ensure database is clean before tests
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        // Clean up after tests
        await _dbContext.Database.EnsureDeletedAsync();
    }

    [Fact]
    public async Task CreateRole_ShouldReturnValidRole()
    {
        // Arrange
        var tenant = await CreateTestTenantAsync();
        Assert.NotNull(tenant);
        
        _tenantContextAccessor.SetCurrentTenantId(tenant.Id);
        string roleName = "Admin";
        string description = "Administrator role";
        
        // Act
        var role = await _permissionService.CreateRoleAsync(roleName, description, true, "system");
        
        // Assert
        Assert.NotNull(role);
        Assert.Equal(roleName, role.Name);
        Assert.Equal(description, role.Description);
        Assert.True(role.IsSystemRole);
        Assert.Equal("system", role.CreatedBy);
        Assert.Equal(tenant.Id, role.TenantId);
    }

    [Fact]
    public async Task CreateRole_ShouldThrowException_WhenNameIsEmpty()
    {
        // Arrange
        var tenant = await CreateTestTenantAsync();
        Assert.NotNull(tenant);
        
        _tenantContextAccessor.SetCurrentTenantId(tenant.Id);
        
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _permissionService.CreateRoleAsync("", "Description", false, "system"));
    }

    [Fact]
    public async Task CreateRole_ShouldThrowException_WhenCreatedByIsEmpty()
    {
        // Arrange
        var tenant = await CreateTestTenantAsync();
        Assert.NotNull(tenant);
        
        _tenantContextAccessor.SetCurrentTenantId(tenant.Id);
        
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _permissionService.CreateRoleAsync("Admin", "Description", false, ""));
    }
    
    [Fact]
    public async Task CreatePermission_ShouldReturnValidPermission()
    {
        // Arrange
        var tenant = await CreateTestTenantAsync();
        _tenantContextAccessor.SetCurrentTenantId(tenant.Id);
        string permissionName = "Create Product";
        string systemName = "Products.Create";
        string description = "Permission to create products";
        string group = "Products";
        
        // Act
        var permission = await _permissionService.CreatePermissionAsync(
            permissionName, 
            systemName, 
            description, 
            group, 
            "system");
        
        // Assert
        Assert.NotNull(permission);
        Assert.Equal(permissionName, permission.Name);
        Assert.Equal(systemName, permission.SystemName);
        Assert.Equal(description, permission.Description);
        Assert.Equal(group, permission.Group);
        Assert.Equal("system", permission.CreatedBy);
        Assert.Equal(tenant.Id, permission.TenantId);
    }
    
    [Fact]
    public async Task AssignPermissionToRole_ShouldCreateValidRolePermission()
    {
        // Arrange
        var (tenant, _, permission) = await CreateTestTenantWithUserAndPermissionAsync();
        var role = await _permissionService.CreateRoleAsync("Test Role", "Test Description", false, "system");
        
        // Make sure we're in the correct tenant context
        _tenantContextAccessor.SetCurrentTenantId(tenant.Id);
        
        // Act
        await _permissionService.AssignPermissionToRoleAsync(role.Id, permission.Id, "system");
        
        // Assert
        var rolePermission = await _dbContext.Set<RolePermission>()
            .FirstOrDefaultAsync(rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id && !rp.IsDeleted);
            
        Assert.NotNull(rolePermission);
        Assert.Equal(role.Id, rolePermission.RoleId);
        Assert.Equal(permission.Id, rolePermission.PermissionId);
        Assert.Equal(tenant.Id, rolePermission.TenantId);
    }
    
    [Fact]
    public async Task RemovePermissionFromRole_ShouldMarkAssociationAsDeleted()
    {
        // Arrange
        var tenant = await CreateTestTenantAsync();
        _tenantContextAccessor.SetCurrentTenantId(tenant.Id);
        var role = await _permissionService.CreateRoleAsync("Admin", "Administrator role", true, "system");
        var permission = await _permissionService.CreatePermissionAsync(
            "Create Product", 
            "Products.Create", 
            "Permission to create products", 
            "Products", 
            "system");
        await _permissionService.AssignPermissionToRoleAsync(role.Id, permission.Id, "system");
        
        // Act
        await _permissionService.RemovePermissionFromRoleAsync(role.Id, permission.Id, "admin");
        
        // Assert
        var rolePermission = await _dbContext.Set<RolePermission>()
            .FirstOrDefaultAsync(rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id);
        Assert.NotNull(rolePermission);
        Assert.True(rolePermission.IsDeleted);
        Assert.Equal("admin", rolePermission.DeletedBy);
        Assert.NotNull(rolePermission.DeletedDate);
    }
    
    [Fact]
    public async Task AssignRoleToUser_ShouldCreateValidUserRole()
    {
        // Arrange
        var (tenant, user, _) = await CreateTestTenantWithUserAndPermissionAsync();
        var role = await _permissionService.CreateRoleAsync("Test Role", "Test Description", false, "system");
        
        // Make sure we're in the correct tenant context
        _tenantContextAccessor.SetCurrentTenantId(tenant.Id);
        
        // Act
        await _permissionService.AssignRoleToUserAsync(Guid.Parse(user.Id), role.Id, "system");
        
        // Assert
        var userRole = await _dbContext.Set<UserRole>()
            .FirstOrDefaultAsync(ur => ur.UserId == Guid.Parse(user.Id) && ur.RoleId == role.Id && !ur.IsDeleted);
            
        Assert.NotNull(userRole);
        Assert.Equal(Guid.Parse(user.Id), userRole.UserId);
        Assert.Equal(role.Id, userRole.RoleId);
        Assert.Equal(tenant.Id, userRole.TenantId);
    }
    
    [Fact]
    public async Task RemoveRoleFromUser_ShouldMarkAssociationAsDeleted()
    {
        // Arrange
        var (tenant, user, _) = await CreateTestTenantWithUserAndPermissionAsync();
        Assert.NotNull(tenant);
        Assert.NotNull(user);
        
        _tenantContextAccessor.SetCurrentTenantId(tenant.Id);
        
        var role = await _permissionService.CreateRoleAsync("TestRole", "Test Role Description", true, "system");
        await _permissionService.AssignRoleToUserAsync(Guid.Parse(user.Id), role.Id, "system");
        
        // Act
        await _permissionService.RemoveRoleFromUserAsync(Guid.Parse(user.Id), role.Id, "system");
        
        // Assert
        var userRole = await _dbContext.Set<UserRole>()
            .FirstOrDefaultAsync(ur => ur.UserId == Guid.Parse(user.Id) && ur.RoleId == role.Id);
        
        Assert.NotNull(userRole);
        Assert.True(userRole.IsDeleted);
        Assert.NotNull(userRole.DeletedDate);
        Assert.Equal("system", userRole.DeletedBy);
    }
    
    [Fact]
    public async Task HasPermission_ShouldReturnTrueWhenUserHasPermission()
    {
        // Arrange
        var (tenant, user, permission) = await CreateTestTenantWithUserAndPermissionAsync();
        
        // Make sure we're in the correct tenant context
        _tenantContextAccessor.SetCurrentTenantId(tenant.Id);
        
        // Act
        var hasPermission = await _permissionService.HasPermissionAsync(Guid.Parse(user.Id), permission.SystemName);
        
        // Assert
        Assert.True(hasPermission);
    }
    
    [Fact]
    public async Task HasPermission_ShouldReturnFalseWhenUserDoesNotHavePermission()
    {
        // Arrange
        var (tenant, user, _) = await CreateTestTenantWithUserAndPermissionAsync();
        
        // Make sure we're in the correct tenant context
        _tenantContextAccessor.SetCurrentTenantId(tenant.Id);
        
        // Act
        var hasPermission = await _permissionService.HasPermissionAsync(Guid.Parse(user.Id), "NonExistentPermission");
        
        // Assert
        Assert.False(hasPermission);
    }
    
    [Fact]
    public async Task HasAnyPermission_ShouldReturnTrueWhenUserHasAnyPermission()
    {
        // Arrange
        var (tenant, user, permission) = await CreateTestTenantWithUserAndPermissionAsync();
        
        // Make sure we're in the correct tenant context
        _tenantContextAccessor.SetCurrentTenantId(tenant.Id);
        
        var permissionNames = new[] { permission.SystemName, "NonExistentPermission" };
        
        // Act
        var hasAnyPermission = await _permissionService.HasAnyPermissionAsync(Guid.Parse(user.Id), permissionNames);
        
        // Assert
        Assert.True(hasAnyPermission);
    }
    
    [Fact]
    public async Task HasAnyPermission_ShouldReturnFalseWhenUserHasNoPermissions()
    {
        // Arrange
        var (tenant, user, _) = await CreateTestTenantWithUserAndPermissionAsync();
        
        // Make sure we're in the correct tenant context
        _tenantContextAccessor.SetCurrentTenantId(tenant.Id);
        
        var permissionNames = new[] { "NonExistentPermission1", "NonExistentPermission2" };
        
        // Act
        var hasAnyPermission = await _permissionService.HasAnyPermissionAsync(Guid.Parse(user.Id), permissionNames);
        
        // Assert
        Assert.False(hasAnyPermission);
    }
    
    [Fact]
    public async Task HasAllPermissions_ShouldReturnFalseWhenUserDoesNotHaveAllPermissions()
    {
        // Arrange
        var (tenant, user, _) = await CreateTestTenantWithUserAndPermissionAsync();
        Assert.NotNull(tenant);
        Assert.NotNull(user);
        
        _tenantContextAccessor.SetCurrentTenantId(tenant.Id);
        
        var role = await _permissionService.CreateRoleAsync("TestRole", "Test Role Description", true, "system");
        await _permissionService.AssignRoleToUserAsync(Guid.Parse(user.Id), role.Id, "system");
        
        var permission1 = await _permissionService.CreatePermissionAsync(
            "Permission 1",
            "Test.Permission1",
            "Test Permission 1",
            "TestGroup",
            "system"
        );
        
        var permission2 = await _permissionService.CreatePermissionAsync(
            "Permission 2",
            "Test.Permission2",
            "Test Permission 2",
            "TestGroup",
            "system"
        );
        
        await _permissionService.AssignPermissionToRoleAsync(role.Id, permission1.Id, "system");
        
        // Act
        var hasAllPermissions = await _permissionService.HasAllPermissionsAsync(
            Guid.Parse(user.Id),
            new[] { permission1.SystemName, permission2.SystemName }
        );
        
        // Assert
        Assert.False(hasAllPermissions);
    }
    
    [Fact]
    public async Task HasAllPermissions_ShouldReturnTrueWhenUserHasAllPermissions()
    {
        // Arrange
        var (tenant, user, _) = await CreateTestTenantWithUserAndPermissionAsync();
        Assert.NotNull(tenant);
        Assert.NotNull(user);
        
        _tenantContextAccessor.SetCurrentTenantId(tenant.Id);
        
        var role = await _permissionService.CreateRoleAsync("TestRole", "Test Role Description", true, "system");
        await _permissionService.AssignRoleToUserAsync(Guid.Parse(user.Id), role.Id, "system");
        
        var permission1 = await _permissionService.CreatePermissionAsync(
            "Permission 1",
            "Test.Permission1",
            "Test Permission 1",
            "TestGroup",
            "system"
        );
        
        var permission2 = await _permissionService.CreatePermissionAsync(
            "Permission 2",
            "Test.Permission2",
            "Test Permission 2",
            "TestGroup",
            "system"
        );
        
        await _permissionService.AssignPermissionToRoleAsync(role.Id, permission1.Id, "system");
        await _permissionService.AssignPermissionToRoleAsync(role.Id, permission2.Id, "system");
        
        // Act
        var hasAllPermissions = await _permissionService.HasAllPermissionsAsync(
            Guid.Parse(user.Id),
            new[] { permission1.SystemName, permission2.SystemName }
        );
        
        // Assert
        Assert.True(hasAllPermissions);
    }
    
    [Fact]
    public async Task GetUserPermissions_ShouldReturnValidPermissions()
    {
        // Arrange
        var (tenant, user, permission) = await CreateTestTenantWithUserAndPermissionAsync();
        var role = await _permissionService.CreateRoleAsync("Test Role", "Test Description", false, "system");
        
        // Make sure we're in the correct tenant context
        _tenantContextAccessor.SetCurrentTenantId(tenant.Id);
        
        // Assign role to user and permission to role
        await _permissionService.AssignRoleToUserAsync(Guid.Parse(user.Id), role.Id, "system");
        await _permissionService.AssignPermissionToRoleAsync(role.Id, permission.Id, "system");
        
        // Act
        var permissions = await _permissionService.GetUserPermissionsAsync(Guid.Parse(user.Id));
        
        // Assert
        Assert.NotNull(permissions);
        Assert.Single(permissions);
        Assert.Equal(permission.Id, permissions.First().Id);
    }
    
    [Fact]
    public async Task GetUserRoles_ShouldReturnValidRoles()
    {
        // Arrange
        var (tenant, user, _) = await CreateTestTenantWithUserAndPermissionAsync();
        var role = await _permissionService.CreateRoleAsync("Test Role", "Test Description", false, "system");
        
        // Make sure we're in the correct tenant context
        _tenantContextAccessor.SetCurrentTenantId(tenant.Id);
        
        // Assign role to user
        await _permissionService.AssignRoleToUserAsync(Guid.Parse(user.Id), role.Id, "system");
        
        // Act
        var roles = await _permissionService.GetUserRolesAsync(Guid.Parse(user.Id));
        
        // Assert
        Assert.NotNull(roles);
        Assert.Single(roles);
        Assert.Equal(role.Id, roles.First().Id);
    }
    
    [Fact]
    public async Task CreateTestTenantWithUserAndPermission_ShouldCreateValidEntities()
    {
        // Arrange
        var tenant = await CreateTestTenantAsync();
        _tenantContextAccessor.SetCurrentTenantId(tenant.Id);
        
        // Act
        var result = await CreateTestTenantWithUserAndPermissionAsync();
        var (testTenant, testUser, testPermission) = result;
        
        // Assert
        Assert.NotNull(testTenant);
        Assert.NotNull(testUser);
        Assert.NotNull(testPermission);
        Assert.Equal(tenant.Id, testTenant.Id);
    }

    [Fact]
    public async Task Permission_ShouldBeIsolatedByTenant()
    {
        // Arrange
        var tenant1 = await CreateTestTenantAsync("Tenant 1", "tenant-1");
        var tenant2 = await CreateTestTenantAsync("Tenant 2", "tenant-2");
        
        // Create test data for tenant 1
        _tenantContextAccessor.SetCurrentTenantId(tenant1.Id);
        var role1 = await _permissionService.CreateRoleAsync("Admin", "Administrator role", true, "system");
        var permission1 = await _permissionService.CreatePermissionAsync(
            "Create Product", 
            "Products.Create", 
            "Permission to create products", 
            "Products", 
            "system");
        await _permissionService.AssignPermissionToRoleAsync(role1.Id, permission1.Id, "system");
        var user1 = await CreateTestUserAsync("user1", "user1@example.com", tenant1.Id);
        await _permissionService.AssignRoleToUserAsync(Guid.Parse(user1.Id), role1.Id, "system");
        
        // Create test data for tenant 2
        _tenantContextAccessor.SetCurrentTenantId(tenant2.Id);
        var role2 = await _permissionService.CreateRoleAsync("Admin", "Administrator role", true, "system");
        var permission2 = await _permissionService.CreatePermissionAsync(
            "Create Product", 
            "Products.Create", 
            "Permission to create products", 
            "Products", 
            "system");
        await _permissionService.AssignPermissionToRoleAsync(role2.Id, permission2.Id, "system");
        var user2 = await CreateTestUserAsync("user2", "user2@example.com", tenant2.Id);
        await _permissionService.AssignRoleToUserAsync(Guid.Parse( user2.Id), role2.Id, "system");
        
        // Act & Assert for tenant 1
        _tenantContextAccessor.SetCurrentTenantId(tenant1.Id);
        var hasPermission1InTenant1 = await _permissionService.HasPermissionAsync(Guid.Parse(user1.Id), permission1.SystemName);
        var hasPermission2InTenant1 = await _permissionService.HasPermissionAsync(Guid.Parse(user1.Id), permission2.SystemName);
        
        Assert.True(hasPermission1InTenant1);
        Assert.False(hasPermission2InTenant1);
        
        // Act & Assert for tenant 2
        _tenantContextAccessor.SetCurrentTenantId(tenant2.Id);
        var hasPermission1InTenant2 = await _permissionService.HasPermissionAsync(Guid.Parse(user2.Id), permission1.SystemName);
        var hasPermission2InTenant2 = await _permissionService.HasPermissionAsync(Guid.Parse(user2.Id), permission2.SystemName);
        
        Assert.False(hasPermission1InTenant2);
        Assert.True(hasPermission2InTenant2);
    }

    [Fact]
    public async Task HasPermission_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        // Arrange
        var tenant = await CreateTestTenantAsync();
        Assert.NotNull(tenant);
        
        _tenantContextAccessor.SetCurrentTenantId(tenant.Id);
        
        // Act
        var hasPermission = await _permissionService.HasPermissionAsync(
            Guid.NewGuid(), // Non-existent user ID
            "Some.Permission"
        );
        
        // Assert
        Assert.False(hasPermission);
    }

    [Fact]
    public async Task HasPermission_ShouldReturnFalse_WhenPermissionDoesNotExist()
    {
        // Arrange
        var (tenant, user, _) = await CreateTestTenantWithUserAndPermissionAsync();
        Assert.NotNull(tenant);
        Assert.NotNull(user);
        
        _tenantContextAccessor.SetCurrentTenantId(tenant.Id);
        
        // Act
        var hasPermission = await _permissionService.HasPermissionAsync(
            Guid.Parse(user.Id),
            "Non.Existent.Permission"
        );
        
        // Assert
        Assert.False(hasPermission);
    }

    [Fact]
    public async Task GetUserPermissions_ShouldReturnEmptyList_WhenUserHasNoRoles()
    {
        // Arrange
        var (tenant, user, _) = await CreateTestTenantWithUserAndPermissionAsync();
        Assert.NotNull(tenant);
        Assert.NotNull(user);
        
        _tenantContextAccessor.SetCurrentTenantId(tenant.Id);
        
        // Act
        var permissions = await _permissionService.GetUserPermissionsAsync(Guid.Parse(user.Id));
        
        // Assert
        Assert.NotNull(permissions);
        Assert.Empty(permissions);
    }
} 