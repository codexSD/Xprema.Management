using System;
using Xprema.Framework.Entities.Common;
using Xprema.Framework.Entities.MultiTenancy;

namespace Xprema.Managment.Domain.ProcedureArea;

public class FlowProcedureCompose : BaseEntity<Guid>, ITenantEntity
{
    public required string ComposeName { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid FlowProcedureStepId { get; set; }
    public Guid TenantId { get; set; }

    // Navigation properties
    public virtual FlowProcedureStep FlowProcedureStep { get; set; } = null!;
}
