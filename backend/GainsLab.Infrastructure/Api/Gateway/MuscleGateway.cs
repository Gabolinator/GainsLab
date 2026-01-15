using GainsLab.Application.DTOs.Muscle;
using GainsLab.Application.Interfaces.DataManagement.Gateway;
using GainsLab.Application.Interfaces.DataManagement.Provider;
using GainsLab.Application.Results;
using GainsLab.Contracts.Dtos.GetDto;

namespace GainsLab.Infrastructure.Api.Gateway;

public class MuscleGateway : IMuscleGateway
{
    private readonly IMuscleProvider _provider;
    
    public  MuscleGateway(IMuscleProvider provider)
    {
        _provider = provider;
    }

    public async Task<Result<IReadOnlyList<MuscleGetDTO>>> GetAllMusclesAsync()
    {
        return Result<IReadOnlyList<MuscleGetDTO>>.NotImplemented(nameof(GetAllMusclesAsync));
    }
}