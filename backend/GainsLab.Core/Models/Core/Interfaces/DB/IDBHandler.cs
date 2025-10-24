using GainsLab.Core.Models.Core.Results;

namespace GainsLab.Core.Models.Core.Interfaces.DB;

public interface IDBHandler
{
    public Task<Result<IDto>> AddOrUpdateAsync(IDto dto);
    public Task<Result<IDto>> AddAsync(IDto dto);
    public Task<Result<IDto>> UpdateAsync(IDto dto);
}