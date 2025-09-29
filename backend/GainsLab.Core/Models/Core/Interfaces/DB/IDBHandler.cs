using GainsLab.Models.Core.Results;
using GainsLab.Models.DataManagement.DB.Model.DTOs;

namespace GainsLab.Infrastructure.DB.Handlers;

public interface IDBHandler
{
    public Task<Result<IDto>> AddOrUpdateAsync(IDto dto);
    public Task<Result<IDto>> AddAsync(IDto dto);
    public Task<Result<IDto>> UpdateAsync(IDto dto);
}