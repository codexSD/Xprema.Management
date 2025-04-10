using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xprema.Framework.Core;
using Xprema.Framework.Entities;
using Xprema.Framework.Entities.MultiTenancy;
using Xprema.Framework.Identity;
using Xprema.Framework.tests;
using Xunit;

namespace Xprema.Framework.tests;

public class TenantContextAccessorTests : TestBase
{
    private readonly TenantContextAccessor<TestDbContext> _tenantContextAccessor;
    private readonly Guid _tenantId;

    public TenantContextAccessorTests()
    {
        _tenantContextAccessor = ServiceProvider.GetRequiredService<TenantContextAccessor<TestDbContext>>();
        _tenantId = Guid.NewGuid();
    }

    [Fact]
    public void GetCurrentTenantId_ShouldReturnEmptyGuidWhenNoTenantIsSet()
    {
        // Act
        var tenantId = _tenantContextAccessor.GetCurrentTenantId();
        
        // Assert
        Assert.Equal(Guid.Empty, tenantId);
    }
    
    [Fact]
    public async Task SetCurrentTenantId_ShouldSetTenantId()
    {
        // Arrange
        var tenantContextAccessor = ServiceProvider.GetRequiredService<TenantContextAccessor<TestDbContext>>();

        // Act
        tenantContextAccessor.SetCurrentTenantId(_tenantId);

        // Assert
        Assert.Equal(_tenantId, tenantContextAccessor.GetCurrentTenantId());
    }
    
    [Fact]
    public async Task GetCurrentTenantId_ShouldReturnSetTenantId()
    {
        // Arrange
        var tenantContextAccessor = ServiceProvider.GetRequiredService<TenantContextAccessor<TestDbContext>>();
        tenantContextAccessor.SetCurrentTenantId(_tenantId);

        // Act
        var currentTenantId = tenantContextAccessor.GetCurrentTenantId();

        // Assert
        Assert.Equal(_tenantId, currentTenantId);
    }
    
    [Fact]
    public async Task GetCurrentTenantAsync_ShouldReturnNullWhenNoTenantIsSet()
    {
        // Act
        var tenant = await _tenantContextAccessor.GetCurrentTenantAsync();
        
        // Assert
        Assert.Null(tenant);
    }
    
    [Fact]
    public async Task GetCurrentTenantAsync_ShouldReturnCorrectTenant()
    {
        // Arrange
        var createdTenant = await CreateTestTenantAsync();
        
        // Make sure the tenant is in the database
        await _dbContext.SaveChangesAsync();
        
        // Verify the tenant can be found directly from the DbContext
        var directlyFoundTenant = await _dbContext.Tenants.FindAsync(createdTenant.Id);
        Assert.NotNull(directlyFoundTenant);
        
        // Set current tenant ID
        _tenantContextAccessor.SetCurrentTenantId(createdTenant.Id);
        
        // Act - Now use the TenantContextAccessor to get the tenant
        var tenant = await _tenantContextAccessor.GetCurrentTenantAsync();
        
        // Assert
        Assert.NotNull(tenant);
        Assert.Equal(createdTenant.Id, tenant.Id);
        Assert.Equal(createdTenant.Name, tenant.Name);
    }
    
    [Fact]
    public void TenantContext_ShouldBeIsolatedBetweenThreads()
    {
        // Arrange
        var tenantId1 = Guid.NewGuid();
        var tenantId2 = Guid.NewGuid();
        var manualResetEvent1 = new ManualResetEventSlim(false);
        var manualResetEvent2 = new ManualResetEventSlim(false);
        Guid? thread1TenantId = null;
        Guid? thread2TenantId = null;
        
        // Act
        var thread1 = new Thread(() =>
        {
            _tenantContextAccessor.SetCurrentTenantId(tenantId1);
            manualResetEvent1.Set(); // Signal thread1 has set the tenant ID
            manualResetEvent2.Wait(); // Wait for thread2 to set its tenant ID
            thread1TenantId = _tenantContextAccessor.GetCurrentTenantId();
        });
        
        var thread2 = new Thread(() =>
        {
            manualResetEvent1.Wait(); // Wait for thread1 to set its tenant ID
            _tenantContextAccessor.SetCurrentTenantId(tenantId2);
            manualResetEvent2.Set(); // Signal thread2 has set its tenant ID
            thread2TenantId = _tenantContextAccessor.GetCurrentTenantId();
        });
        
        thread1.Start();
        thread2.Start();
        
        thread1.Join();
        thread2.Join();
        
        // Assert
        Assert.Equal(tenantId1, thread1TenantId);
        Assert.Equal(tenantId2, thread2TenantId);
        Assert.NotEqual(thread1TenantId, thread2TenantId);
    }

    [Fact]
    public async Task ClearCurrentTenantId_ShouldClearTenantId()
    {
        // Arrange
        var tenantContextAccessor = ServiceProvider.GetRequiredService<TenantContextAccessor<TestDbContext>>();
        tenantContextAccessor.SetCurrentTenantId(_tenantId);

        // Act
        tenantContextAccessor.SetCurrentTenantId(Guid.Empty);

        // Assert
        Assert.Equal(Guid.Empty, tenantContextAccessor.GetCurrentTenantId());
    }
} 