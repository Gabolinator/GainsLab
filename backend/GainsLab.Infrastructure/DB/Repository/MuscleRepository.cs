using GainsLab.Application.Interfaces.DataManagement.Repository;
using GainsLab.Application.Results.APIResults;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.PutDto;
using GainsLab.Contracts.Dtos.UpdateDto;
using GainsLab.Contracts.Dtos.UpdateDto.Outcome;

namespace GainsLab.Infrastructure.DB.Repository;

public class MuscleRepository : IMuscleRepository
{
    public Task<APIResult<MuscleGetDTO>> PullByIdAsync(Guid id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<APIResult<MuscleGetDTO>> PostAsync(MusclePostDTO payload, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<APIResult<MusclePutDTO>> PutAsync(Guid id, MusclePutDTO payload, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<APIResult<MuscleUpdateOutcome>> PatchAsync(Guid id, MuscleUpdateDTO payload, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<APIResult<MuscleGetDTO>> DeleteAsync(Guid id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}