using GainsLab.Contracts.SyncService;
using GainsLab.Core.Models.Core;
using GainsLab.Core.Models.Core.Interfaces.DB;
using Microsoft.AspNetCore.Mvc;

namespace GainsLab.Contracts.Interface;




public interface ISyncService
{
    EntityType EntityType { get; }
    Task PushAsync(CancellationToken ct = default);
    Task<SyncPage<ISyncDto>> PullAsync(SyncCursor cur, int take, CancellationToken ct);
}

public interface ISyncService<T> : ISyncService where T : ISyncDto
{
    new Task<SyncPage<T>> PullAsync(SyncCursor cur, int take, CancellationToken ct);
}


