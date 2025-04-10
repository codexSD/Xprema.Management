using System;
using Xprema.Framework.Entities.Common;
using Xprema.Framework.Entities.MultiTenancy;
using Xprema.Managment.Domain.ActionArea;

namespace Xprema.Managment.Domain.ProcedureArea;

public class FlowProcedureStep : BaseEntity<Guid>, ITenantEntity
{
    public Guid? ComposeId { get; set; }
    public Guid? ProcedureId { get; set; }
    public Guid? ActionId { get; set; }
    public int Step { get; set; }
    public FlowProcedureCompose? ProcedureCompose { get; set; }
    public FlowProcedure? FlowProcedure { get; set; }
    public FlowAction? Action { get; set; }
    public bool? IsDepartment { get; set; }
    public bool? IsSystem { get; set; }
    public string? UserInfo { get; set; }
    public required string StepName { get; set; }
    public string? Description { get; set; }
    public int Order { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid FlowProcedureId { get; set; }
    public Guid TenantId { get; set; }

    // Navigation properties
    public virtual ICollection<FlowProcedureCompose> Composes { get; set; } = new List<FlowProcedureCompose>();
}
