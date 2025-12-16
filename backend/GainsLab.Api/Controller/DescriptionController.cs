
using System.Text.Json;
using GainsLab.Infrastructure.SyncService;
using Microsoft.AspNetCore.Mvc;

namespace GainsLab.Api.Controller;



/*todo add 
[] Put
[] Patch

we wont expose delete as its part of cascading delete of the parent aggregate
*/

/// <summary>
/// ASP.NET Core controller that exposes endpoints for description.
/// </summary>
[ApiController]
[Route("api/descriptions")]
public class DescriptionController :  ControllerBase
{
    private readonly DescriptorSyncService _svc;

    /// <summary>
    /// Initializes a new instance of the <see cref="DescriptorSyncService"/> class.
    /// </summary>
    public DescriptionController(DescriptorSyncService syncService)
    {
        _svc= syncService;
    }
    
    [HttpGet("sync")]
    public async Task<IActionResult> PullAllDescription(
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
        var result = await _svc.PullByIdAsync(id,ct);
        if(!result.Success)  return NotFound();
        
        return Ok(result.Value!);
    }
    
       
    [HttpPost("sync")]
    public async Task<IActionResult> PostDescription(
        JsonElement payload, CancellationToken ct = default)
    {
        var result = await _svc.PostAsync(payload,ct);
        if(!result.Success)  return Unauthorized(); //change error type
        
        return Created();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> PutDescription(
        Guid? guid, JsonElement payload, CancellationToken ct = default)
    {
        var result = await _svc.PutAsync(guid,payload,ct);
        if(!result.Success)  return Unauthorized(); //change error type
        
        return Ok(result.Value!);
    }
    
    [HttpPatch("{id:guid}")] 
    public async Task<IActionResult> PatchDescription(
        Guid? guid, JsonElement payload, CancellationToken ct = default)
    {
        var result = await _svc.PatchAsync(guid,payload,ct);
        if(!result.Success)  return Unauthorized(); //change error type
        
        return Ok(result.Value!);
    }
    
    
  

    
}