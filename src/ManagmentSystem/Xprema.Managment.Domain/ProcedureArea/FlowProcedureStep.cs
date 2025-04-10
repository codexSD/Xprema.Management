using System;
using System.Collections.Generic;
using Xprema.Framework.Entities.Common;
using Xprema.Framework.Entities.MultiTenancy;
using Xprema.Managment.Domain.ActionArea;

namespace Xprema.Managment.Domain.ProcedureArea;

/// <summary>
/// Represents a step within a flow procedure.
/// </summary>
public class FlowProcedureStep : BaseEntity<Guid>, ITenantEntity
{
    /// <summary>
    /// Gets or sets the name of the step.
    /// </summary>
    public required string StepName { get; set; }

    /// <summary>
    /// Gets or sets the description of the step.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the order of the step within the procedure.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this step is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the ID of the flow procedure this step belongs to.
    /// </summary>
    public Guid FlowProcedureId { get; set; }

    /// <summary>
    /// Gets or sets the tenant ID.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the flow procedure this step belongs to.
    /// </summary>
    public virtual FlowProcedure FlowProcedure { get; set; } = null!;

    /// <summary>
    /// Gets or sets the composes associated with this step.
    /// </summary>
    public virtual ICollection<FlowProcedureCompose> Composes { get; set; } = new List<FlowProcedureCompose>();

    protected FlowProcedureStep()
    {
        // Required for EF Core
    }

    public FlowProcedureStep(
        Guid id,
        string stepName,
        Guid flowProcedureId,
        Guid tenantId,
        int order = 0,
        string? description = null)
    {
        Id = id;
        StepName = stepName;
        FlowProcedureId = flowProcedureId;
        TenantId = tenantId;
        Order = order;
        Description = description;
    }
}
