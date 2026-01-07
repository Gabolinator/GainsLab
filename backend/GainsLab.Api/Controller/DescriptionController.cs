
using GainsLab.Application.Interfaces;
using GainsLab.Application.Results.APIResults;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.PutDto;
using GainsLab.Contracts.Dtos.UpdateDto;
using GainsLab.Infrastructure.SyncService;
using Microsoft.AspNetCore.Mvc;

namespace GainsLab.Api.Controller;



/*todo add 
[x] validation 

we wont expose delete as its part of cascading delete of the parent aggregate
*/

/// <summary>
/// ASP.NET Core controller that exposes endpoints for description.
/// </summary>
[ApiController]
[Route("api/descriptions")]
public class DescriptionController :  ControllerBase
{
    private readonly IDescriptorRepository _repo;
    private readonly DescriptorSyncService _svc;
    /// <summary>
    /// Initializes a new instance of the <see cref="DescriptorSyncService"/> class.
    /// </summary>
    public DescriptionController(IDescriptorRepository repo, DescriptorSyncService syncService)
    {
        _svc= syncService;
        _repo = repo;
    }
    
    [HttpGet("sync")]
    public async Task<IActionResult> GetDescriptions(
        [FromQuery] DateTimeOffset? ts, [FromQuery] long? seq, [FromQuery] int take = 200, CancellationToken ct = default)
    {
        
        var cursor = new SyncCursor(ts ?? DateTimeOffset.MinValue, seq ?? 0);
        take = Math.Clamp(take, 1, 500);

        var page = await _svc.PullAsync(cursor, take, ct);
        return Ok(page);
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDescription(
        Guid id, CancellationToken ct = default)
    {
        
        if( id == Guid.Empty)  return BadRequest();
        
        var result = await _repo.PullByIdAsync(id,ct);

        return  APIResultValidation.ValidateResult<DescriptorGetDTO>(this,result);
    }
    
       
    [HttpPost()]
    public async Task<IActionResult> PostDescription(
        [FromBody] DescriptorPostDTO? payload, CancellationToken ct = default)
    {
        
        if(payload == null)  return BadRequest();
        
        var result = await _repo.PostAsync(payload,ct);
   
        return APIResultValidation.ValidateResult<DescriptorGetDTO>(this, result,
            result.Value != null ? new ActionResultInfo(GetActionName(), result.Value.Id) : null);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutDescription(
       Guid id, [FromBody] DescriptorPutDTO? payload  , CancellationToken ct = default)
    {
        if(payload == null)  return BadRequest();
        
        var result = await _repo.PutAsync(id,payload,ct);
        
        return  APIResultValidation.ValidateResult<DescriptorPutDTO>(this,result, new ActionResultInfo(GetActionName(),id));
        
    }
    
    
    [HttpPatch("{id:guid}")] 
    public async Task<IActionResult> PatchDescription(
        Guid id, [FromBody] DescriptorUpdateDTO? payload, CancellationToken ct = default)
    {
        if(payload == null|| id == Guid.Empty)  return BadRequest();
        
        var result = await _repo.PatchAsync(id,payload,ct);
        return  APIResultValidation.ValidateResult< DescriptorUpdateDTO>(this,result);
    
    }


    private string GetActionName() => nameof(GetDescription);


}
