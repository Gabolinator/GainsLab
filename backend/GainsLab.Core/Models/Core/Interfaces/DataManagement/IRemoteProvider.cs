using GainsLab.Core.Models.Core.Interfaces.DB;
using GainsLab.Core.Models.Core.Results;

namespace GainsLab.Core.Models.Core.Interfaces.DataManagement;

public interface IRemoteProvider
{
    public Task<Result> InitializeAsync();

    public Task<Result<ISyncPage<ISyncDto>>> PullAsync(EntityType type, ISyncCursor cursor, int take = 200,
        CancellationToken ct = default) ; 




}