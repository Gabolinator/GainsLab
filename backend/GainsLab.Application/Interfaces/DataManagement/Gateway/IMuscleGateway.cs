using GainsLab.Application.DTOs.Muscle;
using GainsLab.Application.Results;
using GainsLab.Contracts.Dtos.GetDto;

namespace GainsLab.Application.Interfaces.DataManagement.Gateway;

public interface IMuscleGateway
{
    public Task<Result<IReadOnlyList<MuscleGetDTO>>> GetAllMusclesAsync();
}