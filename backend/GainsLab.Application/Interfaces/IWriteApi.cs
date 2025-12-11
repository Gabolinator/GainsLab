using GainsLab.Application.Results;

namespace GainsLab.Application.Interfaces;

public interface IWriteApi<TUpdateDto>
{
    Task<Result> UpdateAsync(TUpdateDto dto, CancellationToken ct = default);
}