using GainsLab.Contracts.Interface;

namespace GainsLab.Application.Interfaces;

public interface IReadApi<TSyncDto>
    where TSyncDto : ISyncDto
{
    Task<TSyncDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
}