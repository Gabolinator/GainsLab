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
[Route("movementcategories")]
public class MovementCategoryController : ControllerBase
{
    private readonly IMovementCategoryRepository _repo;
    private readonly ISyncService<MovementCategorySyncDTO> _svc;
   

    public MovementCategoryController(IMovementCategoryRepository repo, ISyncService<MovementCategorySyncDTO> svc)
    {
        _repo = repo;
        _svc = svc;
    }


     [HttpGet("sync")]
    public async Task<IActionResult> GetCategories(
        [FromQuery] DateTimeOffset? ts, [FromQuery] long? seq, [FromQuery] int take = 200, CancellationToken ct = default)
    {
        
        var cursor = new SyncCursor(ts ?? DateTimeOffset.MinValue, seq ?? 0);
        take = Math.Clamp(take, 1, 500);

        var page = await _svc.PullAsync(cursor, take, ct);
        return Ok(page);
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCategory(
        Guid id, CancellationToken ct = default)
    {
        
        
        if( id == Guid.Empty)  return BadRequest();
        
        var result = await _repo.PullByIdAsync(id,ct);

        return  APIResultValidation.ValidateResult<MovementCategoryGetDTO>(this,result);
    }
    
       
    [HttpPost()]
    public async Task<IActionResult> PostCategory(
        [FromBody] MovementCategoryPostDTO? payload, CancellationToken ct = default)
    {
        
        if(payload == null)  return BadRequest();
        
        var result = await _repo.PostAsync(payload,ct);
   
        return APIResultValidation.ValidateResult<MovementCategoryGetDTO>(this, result,
            result.Value != null ? new ActionResultInfo(GetActionName(), result.Value.Id) : null);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutCategory(
       Guid id, [FromBody] MovementCategoryPutDTO? payload  , CancellationToken ct = default)
    {
        if(payload == null)  return BadRequest();
        
        var result = await _repo.PutAsync(id,payload,ct);
        
        return  APIResultValidation.ValidateResult<MovementCategoryPutDTO>(this,result, new ActionResultInfo(GetActionName(),id));
        
    }
    
    
    [HttpPatch("{id:guid}")] 
    public async Task<IActionResult> PatchCategory(
        Guid id, [FromBody] MovementCategoryUpdateDTO? payload, CancellationToken ct = default)
    {
        if(payload == null|| id == Guid.Empty)  return BadRequest();
        
        var result = await _repo.PatchAsync(id,payload,ct);
        return  APIResultValidation.ValidateResult<MovementCategoryUpdateOutcome>(this,result);
    
    }
    
   

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCategory(Guid id, CancellationToken ct = default)
    {
        if (id == Guid.Empty) return BadRequest();

        var result = await _repo.DeleteAsync(id, ct);
        return APIResultValidation.ValidateResult<MovementCategoryGetDTO>(this, result);
    }

    private string GetActionName() => nameof(GetCategory);
    
}