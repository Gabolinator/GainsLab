using GainsLab.Contracts.Interface;
using Microsoft.AspNetCore.Mvc;

namespace GainsLab.Contracts.SyncService;

public class SyncController : ControllerBase
{
    private readonly ISyncService _sync;
    public SyncController(ISyncService sync) => _sync = sync;

    [HttpPost("sync/push")]
    public async Task<IActionResult> Push() { await _sync.PushAsync(); return Ok(); }
    
}