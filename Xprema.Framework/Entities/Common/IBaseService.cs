using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xprema.Framework.Entities.Common;
using Xprema.Framework.Entities.HistoryFeature;

namespace Xprema.Framework.Entities.Common;

/// <summary>
/// Base interface for all domain services
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
/// <typeparam name="TKey">The primary key type</typeparam>
public interface IBaseService<TEntity, TKey> where TEntity : BaseEntity<TKey>
{
    /// <summary>
    /// Gets an entity by its ID
    /// </summary>
    Task<TEntity?> GetByIdAsync(TKey id);

    /// <summary>
    /// Gets all entities
    /// </summary>
    Task<List<TEntity>> GetAllAsync(bool includeInactive = false);

    /// <summary>
    /// Gets entities by a predicate
    /// </summary>
    Task<List<TEntity>> GetByPredicateAsync(Func<TEntity, bool> predicate, bool includeInactive = false);

    /// <summary>
    /// Gets the first entity matching a predicate
    /// </summary>
    Task<TEntity?> FirstOrDefaultAsync(Func<TEntity, bool> predicate, bool includeInactive = false);

    /// <summary>
    /// Gets the history records for an entity
    /// </summary>
    Task<List<EntityHistoryRecord>> GetHistoryAsync(TKey id);

    /// <summary>
    /// Gets a specific version of an entity at a point in time
    /// </summary>
    Task<TEntity?> GetVersionAsync(TKey id, DateTime pointInTime);
} 