using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xprema.Framework.Entities.Common;

namespace Xprema.Framework.Entities.HistoryFeature;

/// <summary>
/// Service for tracking entity history
/// </summary>
public class EntityHistoryService : IEntityHistoryService
{
    private readonly DbContext _dbContext;

    public EntityHistoryService(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc/>
    public async Task LogEntityCreatedAsync<TEntity>(TEntity entity) where TEntity : BaseEntity<Guid>
    {
        entity.AddHistoryRecord(entity.CreatedBy, "Created");
        await _dbContext.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task LogEntityUpdatedAsync<TEntity>(TEntity entity, Dictionary<string, (object? OldValue, object? NewValue)> propertyChanges) where TEntity : BaseEntity<Guid>
    {
        entity.AddVersionRecord(entity.ModifiedBy ?? "system", "Updated", propertyChanges);
        await _dbContext.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task LogEntityDeletedAsync<TEntity>(TEntity entity) where TEntity : BaseEntity<Guid>
    {
        entity.AddHistoryRecord(entity.DeletedBy ?? "system", "Deleted");
        await _dbContext.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async Task<List<EntityHistoryRecord>> GetEntityHistoryAsync<TEntity>(Guid entityId) where TEntity : BaseEntity<Guid>
    {
        var entity = await _dbContext.Set<TEntity>()
            .Include(e => e.HistoryRecords)
            .ThenInclude(h => h.PropertyChanges)
            .FirstOrDefaultAsync(e => e.Id.Equals(entityId));

        return entity?.HistoryRecords.OrderByDescending(h => h.ChangeDate).ToList() ?? new List<EntityHistoryRecord>();
    }

    /// <inheritdoc/>
    public async Task<TEntity?> GetEntityVersionAsync<TEntity>(Guid entityId, DateTime pointInTime) where TEntity : BaseEntity<Guid>
    {
        var entity = await _dbContext.Set<TEntity>()
            .Include(e => e.HistoryRecords)
            .ThenInclude(h => h.PropertyChanges)
            .FirstOrDefaultAsync(e => e.Id.Equals(entityId));

        if (entity == null)
            return null;

        return entity.GetVersion<TEntity>(pointInTime);
    }
} 