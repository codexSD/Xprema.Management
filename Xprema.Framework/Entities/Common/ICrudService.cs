using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xprema.Framework.Entities.HistoryFeature;

namespace Xprema.Framework.Entities.Common;

/// <summary>
/// Base interface for CRUD operations
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
/// <typeparam name="TKey">The primary key type</typeparam>
/// <typeparam name="TCreateDto">The DTO for creation</typeparam>
/// <typeparam name="TUpdateDto">The DTO for updates</typeparam>
public interface ICrudService<TEntity, TKey, TCreateDto, TUpdateDto> : IBaseService<TEntity, TKey> 
    where TEntity : BaseEntity<TKey>
{
    /// <summary>
    /// Creates a new entity
    /// </summary>
    Task<TEntity> CreateAsync(TCreateDto input);

    /// <summary>
    /// Updates an existing entity
    /// </summary>
    Task<TEntity> UpdateAsync(TKey id, TUpdateDto input);

    /// <summary>
    /// Deletes an entity
    /// </summary>
    Task DeleteAsync(TKey id);

    /// <summary>
    /// Gets the history records for an entity
    /// </summary>
    Task<List<EntityHistoryRecord>> GetHistoryAsync(TKey id);

    /// <summary>
    /// Gets a specific version of an entity at a point in time
    /// </summary>
    Task<TEntity?> GetVersionAsync(TKey id, DateTime pointInTime);
} 