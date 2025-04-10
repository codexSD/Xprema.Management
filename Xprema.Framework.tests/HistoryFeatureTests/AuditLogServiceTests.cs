using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xprema.Framework.Core;
using Xprema.Framework.Entities;
using Xprema.Framework.Entities.HistoryFeature;
using Xprema.Framework.Entities.MultiTenancy;
using Xunit;

namespace Xprema.Framework.tests.HistoryFeatureTests
{
    public class AuditLogServiceTests : TestBase
    {
        private readonly AuditLogService _auditLogService;
        private readonly TenantContextAccessor<TestDbContext> _tenantContextAccessor;
        private new readonly DbContext _dbContext;
        private readonly ILogger<AuditLogService> _logger;

        public AuditLogServiceTests()
        {
            _dbContext = ServiceProvider.GetRequiredService<DbContext>();
            _tenantContextAccessor = ServiceProvider.GetRequiredService<TenantContextAccessor<TestDbContext>>();
            _logger = ServiceProvider.GetRequiredService<ILogger<AuditLogService>>();
            _auditLogService = new AuditLogService(_dbContext, _logger, _tenantContextAccessor);
        }

        [Fact]
        public async Task LogActivity_ShouldCreateAuditLog()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            _tenantContextAccessor.SetCurrentTenantId(tenantId);

            // Act
            await _auditLogService.LogActivityAsync(
                "TestUser",
                "TestAction",
                "TestEntity",
                "TestId",
                "TestDetails",
                "TestMetadata"
            );

            // Assert
            var auditLog = await _dbContext.Set<AuditLog>()
                .FirstOrDefaultAsync(a => a.TenantId == tenantId);
            
            Assert.NotNull(auditLog);
            Assert.Equal("TestAction", auditLog.Activity);
            Assert.Equal("TestEntity", auditLog.EntityType);
            Assert.Equal("TestId", auditLog.EntityId);
            Assert.Equal("TestUser", auditLog.UserId);
            Assert.Equal("TestDetails", auditLog.OldValues);
            Assert.Equal("TestMetadata", auditLog.NewValues);
        }

        [Fact]
        public async Task GetAuditLogs_ShouldReturnFilteredLogs()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            _tenantContextAccessor.SetCurrentTenantId(tenantId);

            await _auditLogService.LogActivityAsync(
                "User1",
                "Action1",
                "Entity1",
                "Id1",
                "Details1",
                "Metadata1"
            );

            await _auditLogService.LogActivityAsync(
                "User2",
                "Action2",
                "Entity2",
                "Id2",
                "Details2",
                "Metadata2"
            );

            // Act
            var logs = await _auditLogService.GetAuditLogsAsync(
                "User1",
                "Entity1",
                "Id1",
                null,
                null,
                0,
                10
            );

            // Assert
            Assert.Single(logs);
            Assert.Equal("Action1", logs.First().Activity);
            Assert.Equal("Entity1", logs.First().EntityType);
            Assert.Equal("Id1", logs.First().EntityId);
        }
    }
} 