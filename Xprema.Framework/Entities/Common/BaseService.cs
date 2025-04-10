using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xprema.Framework.Entities.HistoryFeature;

namespace Xprema.Framework.Entities.Common;

/// <summary>
/// Base implementation for all domain services
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
/// <typeparam name="TKey">The primary key type</typeparam>
public abstract class BaseService<TEntity, TKey> : IBaseService<TEntity, TKey> 
    where TEntity : BaseEntity<TKey>
    where TKey : IEquatable<TKey>
{
    protected virtual DbContext DbContext { get; }
    protected readonly IEntityHistoryService HistoryService;

    protected BaseService(
        DbContext dbContext,
        IEntityHistoryService historyService)
    {
        DbContext = dbContext;
        HistoryService = historyService;
    }

    /// <inheritdoc/>
    public virtual async Task<TEntity?> GetByIdAsync(TKey id)
    {
        return await DbContext.Set<TEntity>()
            .FirstOrDefaultAsync(e => e.Id!.Equals(id));
    }

    /// <inheritdoc/>
    public virtual async Task<List<TEntity>> GetAllAsync(bool includeInactive = false)
    {
        var query = DbContext.Set<TEntity>().AsQueryable();
        
        if (!includeInactive && typeof(IActivable).IsAssignableFrom(typeof(TEntity)))
        {
            query = query.Where(e => ((IActivable)e).IsActive);
        }
        
        return await query.ToListAsync();
    }

    /// <inheritdoc/>
    public virtual async Task<List<TEntity>> GetByPredicateAsync(Func<TEntity, bool> predicate, bool includeInactive = false)
    {
        var query = DbContext.Set<TEntity>().AsQueryable();
        
        if (!includeInactive && typeof(IActivable).IsAssignableFrom(typeof(TEntity)))
        {
            query = query.Where(e => ((IActivable)e).IsActive);
        }
        
        return await query.Where(predicate).AsQueryable().ToListAsync();
    }

    /// <inheritdoc/>
    public virtual async Task<TEntity?> FirstOrDefaultAsync(Func<TEntity, bool> predicate, bool includeInactive = false)
    {
        var query = DbContext.Set<TEntity>().AsQueryable();
        
        if (!includeInactive && typeof(IActivable).IsAssignableFrom(typeof(TEntity)))
        {
            query = query.Where(e => ((IActivable)e).IsActive);
        }
        
        return await query.FirstOrDefaultAsync(e => predicate(e));
    }

    /// <inheritdoc/>
    public virtual async Task<List<EntityHistoryRecord>> GetHistoryAsync(TKey id)
    {
        var entity = await DbContext.Set<TEntity>()
            .Include(e => e.HistoryRecords)
            .ThenInclude(h => h.PropertyChanges)
            .FirstOrDefaultAsync(e => e.Id!.Equals(id));
        
        return entity?.HistoryRecords.OrderByDescending(h => h.ChangeDate).ToList() ?? new List<EntityHistoryRecord>();
    }

    /// <inheritdoc/>
    public virtual async Task<TEntity?> GetVersionAsync(TKey id, DateTime pointInTime)
    {
        var currentEntity = await DbContext.Set<TEntity>()
            .Include(e => e.HistoryRecords)
            .ThenInclude(h => h.PropertyChanges)
            .FirstOrDefaultAsync(e => e.Id!.Equals(id));
        
        if (currentEntity == null)
            return null;
            
        return currentEntity.GetVersion<TEntity>(pointInTime);
    }
} 