using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xprema.Framework.Entities.Common;

namespace Xprema.Managment.Domain.ProcedureArea;

/// <summary>
/// Manager for FlowProcedure domain operations
/// </summary>
public interface IFlowProcedureManager
{
    /// <summary>
    /// Creates a new flow procedure
    /// </summary>
    Task<FlowProcedure> CreateAsync(
        string procedureName,
        Guid tenantId,
        string? description = null,
        string? icon = null,
        bool isSystem = false);

    /// <summary>
    /// Updates an existing flow procedure
    /// </summary>
    Task<FlowProcedure> UpdateAsync(
        Guid id,
        string procedureName,
        string? description = null,
        string? icon = null,
        bool? isSystem = null,
        bool? isActive = null);

    /// <summary>
    /// Deletes a flow procedure
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Gets a flow procedure by id
    /// </summary>
    Task<FlowProcedure?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets all flow procedures for a tenant
    /// </summary>
    Task<List<FlowProcedure>> GetAllAsync(Guid tenantId, bool includeInactive = false);

    /// <summary>
    /// Adds a step to a flow procedure
    /// </summary>
    Task<FlowProcedureStep> AddStepAsync(
        Guid procedureId,
        string stepName,
        int order,
        string? description = null);

    /// <summary>
    /// Removes a step from a flow procedure
    /// </summary>
    Task RemoveStepAsync(Guid procedureId, Guid stepId);
} 