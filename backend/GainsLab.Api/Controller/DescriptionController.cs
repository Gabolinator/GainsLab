
using System.Text.Json;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.PutDto;
using GainsLab.Contracts.Dtos.UpdateDto;
using GainsLab.Infrastructure.DB.Repository;
using GainsLab.Infrastructure.SyncService;
using Microsoft.AspNetCore.Mvc;

namespace GainsLab.Api.Controller;



/*todo add 
[] validation

we wont expose delete as its part of cascading delete of the parent aggregate
*/

/// <summary>
/// ASP.NET Core controller that exposes endpoints for description.
/// </summary>
[ApiController]
[Route("api/descriptions")]
public class DescriptionController :  ControllerBase
{
    private readonly DescriptorRepository _repo;
    private readonly DescriptorSyncService _svc;
    /// <summary>
    /// Initializes a new instance of the <see cref="DescriptorSyncService"/> class.
    /// </summary>
    public DescriptionController(DescriptorSyncService syncService, DescriptorRepository repo)
    {
        _svc= syncService;
        _repo = repo;
    }
    
    [HttpGet("sync")]
    public async Task<IActionResult> PullAllDescriptions(
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
        
        if(id == Guid.Empty)  return BadRequest();
        
        var result = await _repo.PullByIdAsync(id,ct);
        if(!result.Success)  return NotFound();
        
        return Ok(result.Value!);
    }
    
       
    [HttpPost()]
    public async Task<IActionResult> PostDescription(
        [FromBody] DescriptorPostDTO? payload, CancellationToken ct = default)
    {
        if(payload == null)  return BadRequest();
        
        var result = await _repo.PostAsync(payload,ct);
        if(!result.Success)  return Unauthorized(); //change error type
        
        return Created(result.Value?.Id.ToString(), result.Value);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutDescription(
       Guid id, [FromBody] DescriptorPutDTO? payload  , CancellationToken ct = default)
    {
        if(payload == null|| id == Guid.Empty)  return BadRequest();
        
        var result = await _repo.PutAsync(id,payload,ct);
        if(!result.Success)  return NotFound(); //change error type
        
        return Ok(result.Value!);
    }
    
    [HttpPatch("{id:guid}")] 
    public async Task<IActionResult> PatchDescription(
        Guid id, [FromBody] DescriptorUpdateDTO? payload, CancellationToken ct = default)
    {
        if(payload == null|| id == Guid.Empty)  return BadRequest();
        
        var result = await _repo.PatchAsync(id,payload,ct);
        if(!result.Success)  return NotFound(); 
        
        return Ok(result.Value!);
    }
    
    
  

    
}
