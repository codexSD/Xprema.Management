using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xprema.Framework.Entities.Common;
using Xprema.Framework.Entities.HistoryFeature;

namespace Xprema.Managment.Domain.ProcedureArea;

/// <summary>
/// Manager for FlowProcedure domain operations
/// </summary>
public class FlowProcedureManager : IFlowProcedureManager
{
    private readonly DbContext _dbContext;
    private readonly IEntityHistoryService _historyService;

    public FlowProcedureManager(
        DbContext dbContext,
        IEntityHistoryService historyService)
    {
        _dbContext = dbContext;
        _historyService = historyService;
    }

    /// <inheritdoc/>
    public async Task<FlowProcedure> CreateAsync(
        string procedureName,
        Guid tenantId,
        string? description = null,
        string? icon = null,
        bool isSystem = false)
    {
        var procedure = new FlowProcedure(
            Guid.NewGuid(),
            procedureName,
            tenantId,
            description,
            icon,
            isSystem);

        await _dbContext.Set<FlowProcedure>().AddAsync(procedure);
        await _dbContext.SaveChangesAsync();

        await _historyService.LogEntityCreatedAsync(procedure);
        return procedure;
    }

    /// <inheritdoc/>
    public async Task<FlowProcedure> UpdateAsync(
        Guid id,
        string procedureName,
        string? description = null,
        string? icon = null,
        bool? isSystem = null,
        bool? isActive = null)
    {
        var procedure = await GetByIdAsync(id);
        if (procedure == null)
            throw new InvalidOperationException($"FlowProcedure with ID {id} not found");

        var oldValues = new Dictionary<string, (object? OldValue, object? NewValue)>
        {
            { nameof(procedure.ProcedureName), (procedure.ProcedureName, procedureName) },
            { nameof(procedure.Description), (procedure.Description, description) },
            { nameof(procedure.Icon), (procedure.Icon, icon) }
        };

        if (isSystem.HasValue)
            oldValues.Add(nameof(procedure.IsSystem), (procedure.IsSystem, isSystem.Value));
        if (isActive.HasValue)
            oldValues.Add(nameof(procedure.IsActive), (procedure.IsActive, isActive.Value));

        procedure.ProcedureName = procedureName;
        procedure.Description = description;
        procedure.Icon = icon;
        if (isSystem.HasValue)
            procedure.IsSystem = isSystem.Value;
        if (isActive.HasValue)
            procedure.IsActive = isActive.Value;

        _dbContext.Set<FlowProcedure>().Update(procedure);
        await _dbContext.SaveChangesAsync();

        await _historyService.LogEntityUpdatedAsync(procedure, oldValues);
        return procedure;
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Guid id)
    {
        var procedure = await GetByIdAsync(id);
        if (procedure == null)
            throw new InvalidOperationException($"FlowProcedure with ID {id} not found");

        _dbContext.Set<FlowProcedure>().Remove(procedure);
        await _dbContext.SaveChangesAsync();

        await _historyService.LogEntityDeletedAsync(procedure);
    }

    /// <inheritdoc/>
    public async Task<FlowProcedure?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Set<FlowProcedure>()
            .Include(p => p.Steps)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <inheritdoc/>
    public async Task<List<FlowProcedure>> GetAllAsync(Guid tenantId, bool includeInactive = false)
    {
        var query = _dbContext.Set<FlowProcedure>()
            .Include(p => p.Steps)
            .Where(p => p.TenantId == tenantId);

        if (!includeInactive)
            query = query.Where(p => p.IsActive);

        return await query.ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<FlowProcedureStep> AddStepAsync(
        Guid procedureId,
        string stepName,
        int order,
        string? description = null)
    {
        var procedure = await GetByIdAsync(procedureId);
        if (procedure == null)
            throw new InvalidOperationException($"FlowProcedure with ID {procedureId} not found");

        var step = new FlowProcedureStep(
            Guid.NewGuid(),
            stepName,
            procedureId,
            procedure.TenantId,
            order,
            description);

        await _dbContext.Set<FlowProcedureStep>().AddAsync(step);
        await _dbContext.SaveChangesAsync();

        await _historyService.LogEntityCreatedAsync(step);
        return step;
    }

    /// <inheritdoc/>
    public async Task RemoveStepAsync(Guid procedureId, Guid stepId)
    {
        var step = await _dbContext.Set<FlowProcedureStep>()
            .FirstOrDefaultAsync(s => s.Id == stepId && s.FlowProcedureId == procedureId);

        if (step == null)
            throw new InvalidOperationException($"Step with ID {stepId} not found in procedure {procedureId}");

        _dbContext.Set<FlowProcedureStep>().Remove(step);
        await _dbContext.SaveChangesAsync();

        await _historyService.LogEntityDeletedAsync(step);
    }
} 