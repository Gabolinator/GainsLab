using GainsLab.Contracts.Interface;
using GainsLab.Contracts.SyncDto;
using GainsLab.Core.Models.Core;
using GainsLab.Core.Models.Core.Interfaces.DB;
using Microsoft.AspNetCore.Mvc;

namespace GainsLab.Contracts.SyncService;

[ApiController]
[Route("sync")]
public class SyncController : ControllerBase
{
    private readonly IReadOnlyDictionary<EntityType, ISyncService> _services;

    public SyncController(IEnumerable<ISyncService> services)
        => _services = services.ToDictionary(s => s.EntityType);

    
    // GET /sync/{entity}?ts=...&seq=...&take=...
    [HttpGet("{entity}")]
    public async Task<IActionResult> Pull(
        string entity, [FromQuery] DateTimeOffset? ts, [FromQuery] long? seq, [FromQuery] int take = 200, CancellationToken ct = default)
    {
        if (!Enum.TryParse<EntityType>(entity, true, out var t) || !_services.TryGetValue(t, out var svc))
            return NotFound($"Unknown entity '{entity}'.");

        var cursor = new SyncCursor(ts ?? DateTimeOffset.MinValue, seq ?? 0);
        take = Math.Clamp(take, 1, 500);

        var page = await svc.PullAsync(cursor, take, ct); // non-generic call
        return Ok(page);
    }



    // GET /sync/equipment?ts=2025-10-01T00:00:00Z&id=00000000-0000-0000-0000-000000000000&take=200
    // [HttpGet("equipment")]
    // public async Task<ActionResult<SyncPage<EquipmentSyncDto>>> PullEquipment(
    //     [FromQuery] DateTimeOffset? ts,
    //     [FromQuery] long? seq,
    //     [FromQuery] int take = 200,
    //     CancellationToken ct = default)
    // {
    //     var cursor = new SyncCursor(ts ?? DateTimeOffset.MinValue, seq ?? 0);
    //     take = Math.Clamp(take, 1, 500);
    //
    //     var page = await _equipment.PullAsync(cursor, take, ct);
    //     return Ok(page);
    // }
    //
    // [HttpGet("descriptor")]
    // public async Task<ActionResult<SyncPage<DescriptorSyncDto>>> PullDescriptor(
    //     [FromQuery] DateTimeOffset? ts,
    //     [FromQuery] long? seq,
    //     [FromQuery] int take = 200,
    //     CancellationToken ct = default)
    // {
    //     var cursor = new SyncCursor(ts ?? DateTimeOffset.MinValue, seq ?? 0);
    //     take = Math.Clamp(take, 1, 500);
    //
    //     var page = await _descriptor.PullAsync(cursor, take, ct);
    //     return Ok(page);
    // }
    
    
}