using GainsLab.Application.Interfaces.DataManagement.Repository;
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
[Route("movements")]
public class MovementController : ControllerBase
{
    private readonly IMovementRepository _repo;
    private readonly ISyncService<MovementSyncDTO> _svc;
    private readonly ILogger _log;

    public MovementController(IMovementRepository repo, ISyncService<MovementSyncDTO> svc, ILogger log)
    {
        _repo = repo;
        _svc = svc;
        _log = log;
        
    }
    
       [HttpGet("sync")]
    public async Task<IActionResult> GetMovements(
        [FromQuery] DateTimeOffset? ts, [FromQuery] long? seq, [FromQuery] int take = 200, CancellationToken ct = default)
    {
        
        var cursor = new SyncCursor(ts ?? DateTimeOffset.MinValue, seq ?? 0);
        take = Math.Clamp(take, 1, 500);

        var page = await _svc.PullAsync(cursor, take, ct);
        return Ok(page);
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetMovement(
        Guid id, CancellationToken ct = default)
    {
        
        
        if( id == Guid.Empty)  return BadRequest();
        
        var result = await _repo.PullByIdAsync(id,ct);

        return  APIResultValidation.ValidateResult<MovementGetDTO>(this,result);
    }
    
       
    [HttpPost()]
    public async Task<IActionResult> PostMovement(
        [FromBody] MovementPostDTO? payload, CancellationToken ct = default)
    {
        
        if(payload == null)  return BadRequest();
        
        var result = await _repo.PostAsync(payload,ct);
   
        return APIResultValidation.ValidateResult<MovementGetDTO>(this, result,
            result.Value != null ? new ActionResultInfo(GetActionName(), result.Value.Id) : null);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutMovement(
       Guid id, [FromBody] MovementPutDTO? payload  , CancellationToken ct = default)
    {
        if(payload == null)  return BadRequest();
        
        var result = await _repo.PutAsync(id,payload,ct);
        
        return  APIResultValidation.ValidateResult<MovementPutDTO>(this,result, new ActionResultInfo(GetActionName(),id));
        
    }
    
    
    [HttpPatch("{id:guid}")] 
    public async Task<IActionResult> PatchMovement(
        Guid id, [FromBody] MovementUpdateDTO? payload, CancellationToken ct = default)
    {
        if(payload == null|| id == Guid.Empty)  return BadRequest();
        
        var result = await _repo.PatchAsync(id,payload,ct);
        return  APIResultValidation.ValidateResult<MovementUpdateOutcome>(this,result);
    
    }
    
   

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteMovement(Guid id, CancellationToken ct = default)
    {
        if (id == Guid.Empty) return BadRequest();

        var result = await _repo.DeleteAsync(id, ct);
        return APIResultValidation.ValidateResult<MovementGetDTO>(this, result);
    }

    private string GetActionName() => nameof(GetMovement);
}