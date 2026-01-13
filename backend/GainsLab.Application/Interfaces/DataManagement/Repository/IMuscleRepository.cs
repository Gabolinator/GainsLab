using GainsLab.Application.Results.APIResults;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.PutDto;
using GainsLab.Contracts.Dtos.UpdateDto;
using GainsLab.Contracts.Dtos.UpdateDto.Outcome;

namespace GainsLab.Application.Interfaces.DataManagement.Repository;

public interface IMuscleRepository
{
    Task<APIResult<MuscleGetDTO>> PullByIdAsync(Guid id, CancellationToken ct);
    Task<APIResult<MuscleGetDTO>> PostAsync(MusclePostDTO payload, CancellationToken ct);
    Task<APIResult<MusclePutDTO>> PutAsync(Guid id, MusclePutDTO payload, CancellationToken ct);
    Task<APIResult<MuscleUpdateOutcome>> PatchAsync(Guid id, MuscleUpdateDTO payload, CancellationToken ct);
    Task<APIResult<MuscleGetDTO>> DeleteAsync(Guid id, CancellationToken ct);
}