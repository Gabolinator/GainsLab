using System.Text.Json;
using GainsLab.Contracts.Interface;
using GainsLab.Contracts.SyncDto;
using GainsLab.Core.Models.Core;
using GainsLab.Core.Models.Core.Interfaces.DB;
using GainsLab.Core.Models.Core.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace GainsLab.Contracts.SyncService;

/// <summary>
/// ASP.NET Core controller that exposes sync endpoints for consuming clients.
/// </summary>
[ApiController]
[Route("sync")]
public class SyncController : ControllerBase
{
    private readonly IReadOnlyDictionary<EntityType, ISyncService> _services;

    /// <summary>
    /// Initializes a new instance of the <see cref="SyncController"/> class using the registered sync services.
    /// </summary>
    /// <param name="services">The collection of entity-specific sync services provided by DI.</param>
    public SyncController(IEnumerable<ISyncService> services)
        => _services = services.ToDictionary(s => s.EntityType);

    /// <summary>
    /// Retrieves a page of synchronization data for the requested entity type.
    /// </summary>
    /// <param name="entity">Name of the entity type (case-insensitive).</param>
    /// <param name="ts">Timestamp component of the cursor; defaults to the minimum value.</param>
    /// <param name="seq">Sequence component of the cursor; defaults to zero.</param>
    /// <param name="take">Maximum number of items to retrieve; clamped to a safe range.</param>
    /// <param name="ct">Cancellation token propagated from the caller.</param>
    /// <returns>HTTP 200 with the page payload when successful, or HTTP 404 if the entity is unknown.</returns>
    /// <remarks>GET /sync/&lt;entity&gt;?ts=...&amp;seq=...&amp;take=...</remarks>
    [HttpGet("{entity}")]
    public async Task<IActionResult> Pull(
        string entity, [FromQuery] DateTimeOffset? ts, [FromQuery] long? seq, [FromQuery] int take = 200, CancellationToken ct = default)
    {
        if (!Enum.TryParse<EntityType>(entity, true, out var t) || !_services.TryGetValue(t, out var svc))
            return NotFound($"Unknown entity '{entity}'.");

        var cursor = new SyncCursor(ts ?? DateTimeOffset.MinValue, seq ?? 0);
        take = Math.Clamp(take, 1, 500);

        var page = await svc.PullBoxedAsync(cursor, take, ct);
        return Ok(page);
    }
    
    /// <summary>
    /// Accepts a batch of synchronization payloads and forwards them to the appropriate service.
    /// </summary>
    /// <param name="entity">Name of the entity type being pushed.</param>
    /// <param name="body">Envelope containing the client-submitted payloads.</param>
    /// <param name="ct">Cancellation token propagated from the caller.</param>
    /// <returns>HTTP 200 with the push result when successful, or an error status otherwise.</returns>
    [HttpPost("{entity}")]
    public async Task<IActionResult> Push(
        string entity,
        [FromBody] SyncPushEnvelope body,
        CancellationToken ct = default)
    {
        if (!Enum.TryParse<EntityType>(entity, true, out var t) || !_services.TryGetValue(t, out var svc))
            return NotFound($"Unknown entity '{entity}'.");

        CoreUtilities.Logger.Log(nameof(SyncController), $"deserialize entity {t}- {svc.DtoType}" );
        
        // Deserialize each item to the service’s TSyncDto
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var typedItems = new List<ISyncDto>(body.Items.Count);

        foreach (var el in body.Items)
        {
            var dto = (ISyncDto?)JsonSerializer.Deserialize(el, svc.DtoType, options);
            if (dto is null)
                return BadRequest($"Invalid item payload for '{entity}'.");
            typedItems.Add(dto);
        }

        var result = await svc.PushBoxedAsync(typedItems, ct);
        return Ok(result);
    }
}
