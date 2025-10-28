using System.Threading;
using System.Threading.Tasks;
using GainsLab.Core.Models.Core;
using GainsLab.Core.Models.Core.Interfaces.DB;

namespace GainsLab.Models.DataManagement.Sync;

public interface ISyncCursorStore
{
    Task<ISyncCursor?> GetCursorAsync(EntityType type, CancellationToken ct);                                                                                      
    Task SaveCursorAsync(EntityType type, ISyncCursor cursor, CancellationToken ct);      
}