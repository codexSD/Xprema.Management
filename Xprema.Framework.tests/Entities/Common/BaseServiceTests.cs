using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xprema.Framework.Entities.Common;
using Xprema.Framework.Entities.HistoryFeature;
using Xunit;

namespace Xprema.Framework.tests.Entities.Common;

public class BaseServiceTests : TestBase
{
    // Test entity that implements IActivable
    private class TestEntity : BaseEntity<Guid>, IActivable
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
        public bool IsActive { get; set; } = true;
    }

    // Test DbContext
    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }

        public DbSet<TestEntity> TestEntities { get; set; } = null!;
    }

    // Test service implementation
    private class TestService : BaseService<TestEntity, Guid>, ICrudService<TestEntity, Guid, TestEntity, TestEntity>
    {
        public TestService(DbContext dbContext, IEntityHistoryService historyService) 
            : base(dbContext, historyService)
        {
        }

        public async Task<TestEntity> CreateAsync(TestEntity input)
        {
            input.CreatedBy = "test";
            input.CreatedDate = DateTime.UtcNow;
            await DbContext.Set<TestEntity>().AddAsync(input);
            await DbContext.SaveChangesAsync();
            await HistoryService.LogEntityCreatedAsync(input);
            return input;
        }

        public async Task<TestEntity> UpdateAsync(Guid id, TestEntity input)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
                throw new InvalidOperationException($"Entity with ID {id} not found");

            entity.Name = input.Name;
            entity.Value = input.Value;
            entity.IsActive = input.IsActive;
            entity.ModifiedBy = "test";
            entity.ModifiedDate = DateTime.UtcNow;

            var propertyChanges = new Dictionary<string, (object? OldValue, object? NewValue)>
            {
                { nameof(TestEntity.Name), (entity.Name, input.Name) },
                { nameof(TestEntity.Value), (entity.Value, input.Value) },
                { nameof(TestEntity.IsActive), (entity.IsActive, input.IsActive) }
            };

            DbContext.Set<TestEntity>().Update(entity);
            await DbContext.SaveChangesAsync();
            await HistoryService.LogEntityUpdatedAsync(entity, propertyChanges);
            return entity;
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
                throw new InvalidOperationException($"Entity with ID {id} not found");

            entity.IsDeleted = true;
            entity.DeletedBy = "test";
            entity.DeletedDate = DateTime.UtcNow;

            DbContext.Set<TestEntity>().Update(entity);
            await DbContext.SaveChangesAsync();
            await HistoryService.LogEntityDeletedAsync(entity);
        }
    }

    private readonly TestService _service;
    private new readonly DbContext _dbContext;

    public BaseServiceTests()
    {
        _dbContext = ServiceProvider.GetRequiredService<DbContext>();
        _service = new TestService(_dbContext, _historyServiceMock.Object);
    }

    [Fact]
    public async Task Create_ShouldCreateEntity()
    {
        // Arrange
        var entity = new TestEntity 
        { 
            Id = Guid.NewGuid(),
            Name = "Test" 
        };

        // Act
        var result = await _service.CreateAsync(entity);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test", result.Name);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task GetById_ShouldReturnEntity()
    {
        // Arrange
        var entity = new TestEntity 
        { 
            Id = Guid.NewGuid(),
            Name = "Test" 
        };
        var created = await _service.CreateAsync(entity);

        // Act
        var result = await _service.GetByIdAsync(created.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal("Test", result.Name);
    }

    [Fact]
    public async Task Update_ShouldUpdateEntity()
    {
        // Arrange
        var entity = new TestEntity 
        { 
            Id = Guid.NewGuid(),
            Name = "Test" 
        };
        var created = await _service.CreateAsync(entity);

        // Act
        created.Name = "Updated";
        var result = await _service.UpdateAsync(created.Id, created);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated", result.Name);
    }

    [Fact]
    public async Task Delete_ShouldMarkEntityAsDeleted()
    {
        // Arrange
        var entity = new TestEntity 
        { 
            Id = Guid.NewGuid(),
            Name = "Test" 
        };
        var created = await _service.CreateAsync(entity);

        // Act
        await _service.DeleteAsync(created.Id);

        // Assert
        var result = await _service.GetByIdAsync(created.Id);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_WithInactiveEntity_ShouldFilterInactiveByDefault()
    {
        // Arrange
        var activeEntity = new TestEntity
        {
            Id = Guid.NewGuid(),
            Name = "Active Entity",
            IsActive = true
        };
        var inactiveEntity = new TestEntity
        {
            Id = Guid.NewGuid(),
            Name = "Inactive Entity",
            IsActive = false
        };
        await _service.CreateAsync(activeEntity);
        await _service.CreateAsync(inactiveEntity);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal(activeEntity.Id, result[0].Id);
    }

    [Fact]
    public async Task GetHistoryAsync_ShouldReturnOrderedHistory()
    {
        // Arrange
        var entity = new TestEntity
        {
            Id = Guid.NewGuid(),
            Name = "Initial Name",
            Value = 10
        };

        // Add initial entity
        await _service.CreateAsync(entity);

        // Update entity
        entity.Name = "Updated Name";
        entity.Value = 20;
        await _service.UpdateAsync(entity.Id, entity);

        // Act
        var history = await _service.GetHistoryAsync(entity.Id);

        // Assert
        Assert.Equal(2, history.Count);
        Assert.Equal("Updated", history[0].ChangeType); // Most recent first
        Assert.Equal("Created", history[1].ChangeType);
    }

    [Fact]
    public async Task GetVersionAsync_ShouldReturnEntityAtPointInTime()
    {
        // Arrange
        var entity = new TestEntity
        {
            Id = Guid.NewGuid(),
            Name = "Initial Name",
            Value = 10
        };

        // Add initial entity
        var creationTime = DateTime.UtcNow;
        await _service.CreateAsync(entity);

        // Update entity after a delay
        await Task.Delay(100); // Ensure different timestamps
        var updateTime = DateTime.UtcNow;
        entity.Name = "Updated Name";
        entity.Value = 20;
        await _service.UpdateAsync(entity.Id, entity);

        // Act
        var originalVersion = await _service.GetVersionAsync(entity.Id, creationTime);
        var updatedVersion = await _service.GetVersionAsync(entity.Id, updateTime);

        // Assert
        Assert.NotNull(originalVersion);
        Assert.NotNull(updatedVersion);
        Assert.Equal("Initial Name", originalVersion.Name);
        Assert.Equal(10, originalVersion.Value);
        Assert.Equal("Updated Name", updatedVersion.Name);
        Assert.Equal(20, updatedVersion.Value);
    }

    [Fact]
    public async Task GetByPredicateAsync_ShouldFilterByPredicate()
    {
        // Arrange
        var entities = new[]
        {
            new TestEntity { Id = Guid.NewGuid(), Name = "Entity 1", Value = 10 },
            new TestEntity { Id = Guid.NewGuid(), Name = "Entity 2", Value = 20 },
            new TestEntity { Id = Guid.NewGuid(), Name = "Entity 3", Value = 30 }
        };
        foreach (var entity in entities)
        {
            await _service.CreateAsync(entity);
        }

        // Act
        var result = await _service.GetByPredicateAsync(e => e.Value > 15);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, e => Assert.True(e.Value > 15));
    }

    [Fact]
    public async Task FirstOrDefaultAsync_ShouldReturnFirstMatchingEntity()
    {
        // Arrange
        var entities = new[]
        {
            new TestEntity { Id = Guid.NewGuid(), Name = "Entity 1", Value = 10 },
            new TestEntity { Id = Guid.NewGuid(), Name = "Entity 2", Value = 20 },
            new TestEntity { Id = Guid.NewGuid(), Name = "Entity 3", Value = 20 }
        };
        foreach (var entity in entities)
        {
            await _service.CreateAsync(entity);
        }

        // Act
        var result = await _service.FirstOrDefaultAsync(e => e.Value == 20);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(20, result.Value);
        Assert.Equal(entities[1].Id, result.Id); // Should be the first one with Value = 20
    }
} 