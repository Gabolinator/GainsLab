using GainsLab.Contracts.Interface;

namespace GainsLab.Application.Interfaces;

public interface IEntityApi<TSyncDto, TUpdateDto> 
    : IReadApi<TSyncDto>, IWriteApi<TUpdateDto>
    where TSyncDto : ISyncDto
    where TUpdateDto : class
{
}