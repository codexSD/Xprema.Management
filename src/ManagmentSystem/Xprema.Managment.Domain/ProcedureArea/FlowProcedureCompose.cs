using System;
using Xprema.Framework.Entities.Common;
using Xprema.Framework.Entities.MultiTenancy;

namespace Xprema.Managment.Domain.ProcedureArea;

/// <summary>
/// Represents a composition within a flow procedure step.
/// </summary>
public class FlowProcedureCompose : BaseEntity<Guid>, ITenantEntity
{
    /// <summary>
    /// Gets or sets the name of the compose.
    /// </summary>
    public required string ComposeName { get; set; }

    /// <summary>
    /// Gets or sets the description of the compose.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this compose is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the ID of the flow procedure step this compose belongs to.
    /// </summary>
    public Guid FlowProcedureStepId { get; set; }

    /// <summary>
    /// Gets or sets the tenant ID.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the flow procedure step this compose belongs to.
    /// </summary>
    public virtual FlowProcedureStep FlowProcedureStep { get; set; } = null!;

    protected FlowProcedureCompose()
    {
        // Required for EF Core
    }

    public FlowProcedureCompose(
        Guid id,
        string composeName,
        Guid flowProcedureStepId,
        Guid tenantId,
        string? description = null)
    {
        Id = id;
        ComposeName = composeName;
        FlowProcedureStepId = flowProcedureStepId;
        TenantId = tenantId;
        Description = description;
    }
}
