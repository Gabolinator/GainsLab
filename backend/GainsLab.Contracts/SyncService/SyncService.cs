using GainsLab.Contracts.Interface;
using GainsLab.Infrastructure.DB.Context;

namespace GainsLab.Contracts.SyncService;

public class SyncService : ISyncService
{
    private readonly GainLabPgDBContext _db;      // server-side
    private readonly ILogger<SyncService> _log;

    public SyncService(GainLabPgDBContext db, ILogger<SyncService> log)
    {
        _db = db; _log = log;
    }

    public Task PushAsync(CancellationToken ct = default) { /* ... */ return Task.CompletedTask; }
    public Task PullAsync(DateTimeOffset since, CancellationToken ct = default) { /* ... */ return Task.CompletedTask; }
}