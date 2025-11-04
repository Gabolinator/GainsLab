using GainsLab.Core.Models.Core.Interfaces.Entity;
using GainsLab.Core.Models.Core.Results;

namespace GainsLab.Core.Models.Core.Interfaces.DB;

public interface IDBHandler
{
    public Task<Result<IReadOnlyList<IDto>>> AddOrUpdateAsync(IReadOnlyList<IDto> dto, bool save ,CancellationToken ct);
    public Task<Result<IDto>> AddOrUpdateAsync(IDto dto, bool save,CancellationToken ct = default);
    public Task<Result<IDto>> AddAsync(IDto dto, bool save,CancellationToken ct = default);
    public Task<Result<IDto>> UpdateAsync(IDto dto, bool save,CancellationToken ct = default);
    public Task<IReadOnlyList<IEntity>> GetAllEntityAsync(CancellationToken ct = default);

}