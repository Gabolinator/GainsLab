using System.Threading.Tasks;
using GainsLab.Models.Core.Results;
using GainsLab.Models.DataManagement.DB.Model.DTOs;

namespace GainsLab.Models.DataManagement.DB.Model.Handlers;

public interface IDBHandler
{
    public Task<Result<IDto>> AddOrUpdateAsync(IDto dto);
    public Task<Result<IDto>> AddAsync(IDto dto);
    public Task<Result<IDto>> UpdateAsync(IDto dto);
}