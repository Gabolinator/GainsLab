
using GainsLab.Infrastructure.SyncService;
using Microsoft.AspNetCore.Mvc;

namespace GainsLab.Api.Controller;



/*todo add 
[] Put
[] Patch
[] Delete
do i keep 
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
    public async Task<IActionResult> PullAll(
        [FromQuery] DateTimeOffset? ts, [FromQuery] long? seq, [FromQuery] int take = 200, CancellationToken ct = default)
    {
        
        var cursor = new SyncCursor(ts ?? DateTimeOffset.MinValue, seq ?? 0);
        take = Math.Clamp(take, 1, 500);

        var page = await _svc.PullAsync(cursor, take, ct);
        return Ok(page);
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Pull(
        Guid id, CancellationToken ct = default)
    {
        var result = await _svc.PullById(id,ct);
        if(!result.Success)  return NotFound();
        
        return Ok(result.Value!);
    }
    
    
    
    
}