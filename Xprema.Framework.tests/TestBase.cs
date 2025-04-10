using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xprema.Framework.Entities.Common;
using Xprema.Framework.Entities.HistoryFeature;
using Xprema.Framework.Entities.Identity;
using Xprema.Framework.Entities.MultiTenancy;
using Xprema.Framework.Entities.Permission;
using Xprema.Framework.Identity;

namespace Xprema.Framework.tests;

public abstract class TestBase
{
    protected readonly TestDbContext _dbContext;
    protected readonly Mock<IEntityHistoryService> _historyServiceMock;
    protected readonly Guid _tenantId = Guid.NewGuid();
    protected readonly string _userId = Guid.NewGuid().ToString();
    protected readonly IServiceProvider ServiceProvider;

    protected TestBase()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();

        _dbContext = ServiceProvider.GetRequiredService<TestDbContext>();
        _historyServiceMock = new Mock<IEntityHistoryService>();
        SetupHistoryServiceMock();
        InitializeTestData();
    }

    private void SetupHistoryServiceMock()
    {
        _historyServiceMock.Setup(x => x.LogEntityCreatedAsync(It.IsAny<BaseEntity<Guid>>()))
            .Returns(Task.CompletedTask);
        _historyServiceMock.Setup(x => x.LogEntityUpdatedAsync(
            It.IsAny<BaseEntity<Guid>>(),
            It.IsAny<Dictionary<string, (object? OldValue, object? NewValue)>>()))
            .Returns(Task.CompletedTask);
        _historyServiceMock.Setup(x => x.LogEntityDeletedAsync(It.IsAny<BaseEntity<Guid>>()))
            .Returns(Task.CompletedTask);
        _historyServiceMock.Setup(x => x.GetEntityHistoryAsync<BaseEntity<Guid>>(It.IsAny<Guid>()))
            .ReturnsAsync(new List<EntityHistoryRecord>());
        _historyServiceMock.Setup(x => x.GetEntityVersionAsync<BaseEntity<Guid>>(It.IsAny<Guid>(), It.IsAny<DateTime>()))
            .ReturnsAsync((BaseEntity<Guid>?)null);
    }

    protected virtual void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<TestDbContext>(options =>
            options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));

        // Register TenantContextAccessor with proper type argument
        services.AddScoped<TenantContextAccessor<TestDbContext>>();
        
        // Register PermissionService with proper type argument
        services.AddScoped<PermissionService<TestDbContext>>();
        
        // Register other required services
        services.AddScoped<IEntityHistoryService>(_ => _historyServiceMock.Object);
        
        // Register Identity services
        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<TestDbContext>()
            .AddDefaultTokenProviders();
            
        // Register other framework services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
    }

    protected virtual void InitializeTestData()
    {
        // Create test tenant
        var tenant = new Tenant
        {
            Id = _tenantId,
            Name = "Test Tenant",
            Identifier = "test-tenant",
            IsActive = true,
            Settings = new Dictionary<string, string>()
        };
        _dbContext.Tenants.Add(tenant);

        // Create test user
        var user = new ApplicationUser
        {
            Id = _userId,
            UserName = "testuser",
            Email = "test@example.com",
            IsActive = true,
            TenantId = _tenantId.ToString()
        };
        _dbContext.Users.Add(user);

        // Create test role
        var roleId = Guid.NewGuid();
        var role = new Role
        {
            Id = roleId,
            Name = "Test Role",
            Description = "Test role description",
            IsSystemRole = false,
            TenantId = _tenantId
        };
        _dbContext.Set<Role>().Add(role);

        // Create test permission
        var permission = new Permission
        {
            Id = Guid.NewGuid(),
            Name = "Test Permission",
            SystemName = "test.permission",
            Description = "Test permission description",
            Group = "Test",
            TenantId = _tenantId
        };
        _dbContext.Set<Permission>().Add(permission);

        // Create test role permission
        var rolePermission = new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = roleId,
            PermissionId = permission.Id,
            TenantId = _tenantId
        };
        _dbContext.Set<RolePermission>().Add(rolePermission);

        // Create test user role
        var userRole = new Xprema.Framework.Entities.Permission.UserRole
        {
            Id = Guid.NewGuid(),
            UserId = Guid.Parse(_userId),
            RoleId = roleId,
            TenantId = _tenantId
        };
        _dbContext.Set<Xprema.Framework.Entities.Permission.UserRole>().Add(userRole);

        _dbContext.SaveChanges();
    }

    protected async Task<Tenant> CreateTestTenantAsync(string name = "Test Tenant", string identifier = "test-tenant")
    {
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = name,
            Identifier = identifier,
            IsActive = true,
            Settings = new Dictionary<string, string>()
        };
        _dbContext.Tenants.Add(tenant);
        await _dbContext.SaveChangesAsync();
        return tenant;
    }

    protected async Task<ApplicationUser> CreateTestUserAsync(string userName = "testuser", string email = "test@example.com", Guid? tenantId = null)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = userName,
            Email = email,
            IsActive = true,
            TenantId = (tenantId ?? _tenantId).ToString()
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        return user;
    }

    protected async Task<(Tenant Tenant, ApplicationUser User, Permission Permission)> CreateTestTenantWithUserAndPermissionAsync()
    {
        var tenant = await CreateTestTenantAsync();
        var user = await CreateTestUserAsync(tenantId: tenant.Id);
        
        var permission = new Permission
        {
            Id = Guid.NewGuid(),
            Name = "Test Permission",
            SystemName = "test.permission",
            Group = "Test",
            Description = "Test permission description"
        };
        _dbContext.Permissions.Add(permission);
        await _dbContext.SaveChangesAsync();

        return (tenant, user, permission);
    }

    protected async Task<List<EntityHistoryRecord>> GetHistoryAsync<TEntity>(Guid id) where TEntity : BaseEntity<Guid>
    {
        return await _historyServiceMock.Object.GetEntityHistoryAsync<TEntity>(id);
    }

    protected async Task<TEntity?> GetVersionAsync<TEntity>(Guid id, DateTime pointInTime) where TEntity : BaseEntity<Guid>
    {
        return await _historyServiceMock.Object.GetEntityVersionAsync<TEntity>(id, pointInTime);
    }

    protected async Task<TEntity> UpdateEntityAsync<TEntity>(TEntity entity, Dictionary<string, (object? OldValue, object? NewValue)> propertyChanges) where TEntity : BaseEntity<Guid>
    {
        _dbContext.Set<TEntity>().Update(entity);
        await _dbContext.SaveChangesAsync();
        await _historyServiceMock.Object.LogEntityUpdatedAsync(entity, propertyChanges);
        return entity;
    }

    protected async Task DeleteEntityAsync<TEntity>(TEntity entity) where TEntity : BaseEntity<Guid>
    {
        _dbContext.Set<TEntity>().Remove(entity);
        await _dbContext.SaveChangesAsync();
        await _historyServiceMock.Object.LogEntityDeletedAsync(entity);
    }
} 