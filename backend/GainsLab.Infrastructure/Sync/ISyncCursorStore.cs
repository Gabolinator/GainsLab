using GainsLab.Contracts.Interface;
using GainsLab.Domain;


namespace GainsLab.Infrastructure.Sync;

public interface ISyncCursorStore
{
    Task<ISyncCursor?> GetCursorAsync(EntityType type, CancellationToken ct);                                                                                      
    Task SaveCursorAsync(EntityType type, ISyncCursor cursor, CancellationToken ct);      
}