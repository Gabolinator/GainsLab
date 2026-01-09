using GainsLab.Application.Interfaces;
using GainsLab.Application.Results.APIResults;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.PutDto;
using GainsLab.Contracts.Dtos.SyncDto;
using GainsLab.Contracts.Dtos.UpdateDto;
using GainsLab.Contracts.Dtos.UpdateDto.Outcome;
using GainsLab.Infrastructure.SyncService;
using Microsoft.AspNetCore.Mvc;


namespace GainsLab.Api.Controller;

[ApiController]
[Route("equipments")]
public class EquipmentController : ControllerBase
{
    private readonly IEquipmentRepository _repo;
    private readonly ISyncService<EquipmentSyncDTO> _svc;
   

    public EquipmentController(IEquipmentRepository repo, ISyncService<EquipmentSyncDTO> svc)
    {
        _repo = repo;
        _svc = svc;
    }


     [HttpGet("sync")]
    public async Task<IActionResult> GetEquipments(
        [FromQuery] DateTimeOffset? ts, [FromQuery] long? seq, [FromQuery] int take = 200, CancellationToken ct = default)
    {
        
        var cursor = new SyncCursor(ts ?? DateTimeOffset.MinValue, seq ?? 0);
        take = Math.Clamp(take, 1, 500);

        var page = await _svc.PullAsync(cursor, take, ct);
        return Ok(page);
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetEquipment(
        Guid id, CancellationToken ct = default)
    {
        
        
        if( id == Guid.Empty)  return BadRequest();
        
        var result = await _repo.PullByIdAsync(id,ct);

        return  APIResultValidation.ValidateResult<EquipmentGetDTO>(this,result);
    }
    
       
    [HttpPost()]
    public async Task<IActionResult> PostEquipment(
        [FromBody] EquipmentPostDTO? payload, CancellationToken ct = default)
    {
        
        if(payload == null)  return BadRequest();
        
        var result = await _repo.PostAsync(payload,ct);
   
        return APIResultValidation.ValidateResult<EquipmentGetDTO>(this, result,
            result.Value != null ? new ActionResultInfo(GetActionName(), result.Value.Id) : null);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutEquipment(
       Guid id, [FromBody] EquipmentPutDTO? payload  , CancellationToken ct = default)
    {
        if(payload == null)  return BadRequest();
        
        var result = await _repo.PutAsync(id,payload,ct);
        
        return  APIResultValidation.ValidateResult<EquipmentPutDTO>(this,result, new ActionResultInfo(GetActionName(),id));
        
    }
    
    
    [HttpPatch("{id:guid}")] 
    public async Task<IActionResult> PatchEquipment(
        Guid id, [FromBody] EquipmentUpdateDTO? payload, CancellationToken ct = default)
    {
        if(payload == null|| id == Guid.Empty)  return BadRequest();
        
        var result = await _repo.PatchAsync(id,payload,ct);
        return  APIResultValidation.ValidateResult<EquipmentUpdateOutcome>(this,result);
    
    }
    
   

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteEquipment(Guid id, CancellationToken ct = default)
    {
        if (id == Guid.Empty) return BadRequest();

        var result = await _repo.DeleteAsync(id, ct);
        return APIResultValidation.ValidateResult<EquipmentGetDTO>(this, result);
    }

    private string GetActionName() => nameof(GetEquipment);
    
}
