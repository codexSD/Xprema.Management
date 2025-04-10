using System;
using System.Collections.Generic;
using Xprema.Framework.Entities.Common;
using Xprema.Framework.Entities.MultiTenancy;
using Xprema.Managment.Domain.ActionArea;
using Xprema.Managment.Domain.TaskArea;

namespace Xprema.Managment.Domain.ProcedureArea;

/// <summary>
/// Represents a flow procedure in the system.
/// </summary>
public class FlowProcedure : BaseEntity<Guid>, ITenantEntity
{
    /// <summary>
    /// Gets or sets the name of the procedure.
    /// </summary>
    public required string ProcedureName { get; set; }

    /// <summary>
    /// Gets or sets the description of the procedure.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the icon for the procedure.
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this procedure is a system procedure.
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this procedure is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the concurrency stamp.
    /// </summary>
    public string? ConcurrencyStamp { get; set; }

    /// <summary>
    /// Gets or sets the tenant ID.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the steps of this procedure.
    /// </summary>
    public virtual ICollection<FlowProcedureStep> Steps { get; set; } = new List<FlowProcedureStep>();

    /// <summary>
    /// Gets or sets the tasks associated with this procedure.
    /// </summary>
    public virtual ICollection<FlowTask> Tasks { get; set; } = new List<FlowTask>();

    protected FlowProcedure()
    {
        // Required for EF Core
    }

    public FlowProcedure(
        Guid id,
        string procedureName,
        Guid tenantId,
        string? description = null,
        string? icon = null,
        bool isSystem = false)
    {
        Id = id;
        ProcedureName = procedureName;
        TenantId = tenantId;
        Description = description;
        Icon = icon;
        IsSystem = isSystem;
    }
}

