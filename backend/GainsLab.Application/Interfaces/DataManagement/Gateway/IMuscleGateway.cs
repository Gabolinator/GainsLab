using GainsLab.Application.DTOs.Muscle;
using GainsLab.Application.Results;

namespace GainsLab.Application.Interfaces.DataManagement.Gateway;

public interface IMuscleGateway
{
    public Task<Result<IReadOnlyList<MuscleRecord>>> GetAllMusclesAsync();
}