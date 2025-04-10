using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Xprema.Framework.Entities.Common;
using Xprema.Managment.Domain.ProcedureArea;

namespace Xprema.Managment.HttpApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class FlowProcedureController : ControllerBase
{
    private readonly IFlowProcedureManager _flowProcedureManager;
    private readonly ITenantContextAccessor _tenantContextAccessor;

    public FlowProcedureController(
        IFlowProcedureManager flowProcedureManager,
        ITenantContextAccessor tenantContextAccessor)
    {
        _flowProcedureManager = flowProcedureManager;
        _tenantContextAccessor = tenantContextAccessor;
    }

    [HttpGet]
    public async Task<ActionResult<List<FlowProcedure>>> GetAll(bool includeInactive = false)
    {
        var tenantId = _tenantContextAccessor.GetCurrentTenantId();
        var procedures = await _flowProcedureManager.GetAllAsync(tenantId, includeInactive);
        return Ok(procedures);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FlowProcedure>> GetById(Guid id)
    {
        var procedure = await _flowProcedureManager.GetByIdAsync(id);
        if (procedure == null)
            return NotFound();

        return Ok(procedure);
    }

    [HttpPost]
    public async Task<ActionResult<FlowProcedure>> Create([FromBody] CreateFlowProcedureDto input)
    {
        var tenantId = _tenantContextAccessor.GetCurrentTenantId();
        var procedure = await _flowProcedureManager.CreateAsync(
            input.ProcedureName,
            tenantId,
            input.Description,
            input.Icon,
            input.IsSystem);

        return CreatedAtAction(nameof(GetById), new { id = procedure.Id }, procedure);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<FlowProcedure>> Update(Guid id, [FromBody] UpdateFlowProcedureDto input)
    {
        try
        {
            var procedure = await _flowProcedureManager.UpdateAsync(
                id,
                input.ProcedureName,
                input.Description,
                input.Icon,
                input.IsSystem,
                input.IsActive);

            return Ok(procedure);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            await _flowProcedureManager.DeleteAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("{procedureId}/steps")]
    public async Task<ActionResult<FlowProcedureStep>> AddStep(Guid procedureId, [FromBody] AddStepDto input)
    {
        try
        {
            var step = await _flowProcedureManager.AddStepAsync(
                procedureId,
                input.StepName,
                input.Order,
                input.Description);

            return CreatedAtAction(nameof(GetById), new { id = procedureId }, step);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{procedureId}/steps/{stepId}")]
    public async Task<ActionResult> RemoveStep(Guid procedureId, Guid stepId)
    {
        try
        {
            await _flowProcedureManager.RemoveStepAsync(procedureId, stepId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }
}

public class CreateFlowProcedureDto
{
    public required string ProcedureName { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public bool IsSystem { get; set; }
}

public class UpdateFlowProcedureDto
{
    public required string ProcedureName { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public bool? IsSystem { get; set; }
    public bool? IsActive { get; set; }
}

public class AddStepDto
{
    public required string StepName { get; set; }
    public int Order { get; set; }
    public string? Description { get; set; }
} 