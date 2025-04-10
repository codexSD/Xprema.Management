using System.Collections.Generic;
using System.Threading.Tasks;
using Xprema.Framework.Entities.Common;

namespace Xprema.Framework.Entities.HistoryFeature;

/// <summary>
/// Service for tracking entity history
/// </summary>
public interface IEntityHistoryService
{
    /// <summary>
    /// Logs entity creation
    /// </summary>
    Task LogEntityCreatedAsync<TEntity>(TEntity entity) where TEntity : BaseEntity<Guid>;

    /// <summary>
    /// Logs entity update with property changes
    /// </summary>
    Task LogEntityUpdatedAsync<TEntity>(TEntity entity, Dictionary<string, (object? OldValue, object? NewValue)> propertyChanges) where TEntity : BaseEntity<Guid>;

    /// <summary>
    /// Logs entity deletion
    /// </summary>
    Task LogEntityDeletedAsync<TEntity>(TEntity entity) where TEntity : BaseEntity<Guid>;

    /// <summary>
    /// Gets the history records for an entity
    /// </summary>
    Task<List<EntityHistoryRecord>> GetEntityHistoryAsync<TEntity>(Guid entityId) where TEntity : BaseEntity<Guid>;

    /// <summary>
    /// Gets a specific version of an entity at a point in time
    /// </summary>
    Task<TEntity?> GetEntityVersionAsync<TEntity>(Guid entityId, DateTime pointInTime) where TEntity : BaseEntity<Guid>;
} 