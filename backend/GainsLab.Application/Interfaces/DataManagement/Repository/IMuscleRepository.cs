using GainsLab.Application.Results.APIResults;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.PutDto;
using GainsLab.Contracts.Dtos.UpdateDto;
using GainsLab.Contracts.Dtos.UpdateDto.Outcome;
using GainsLab.Domain.Entities.Identifier;

namespace GainsLab.Application.Interfaces.DataManagement.Repository;

public interface IMuscleRepository
{
    Task<APIResult<MuscleGetDTO>> PullByIdAsync(MuscleId id, CancellationToken ct);
    Task<APIResult<MuscleGetDTO>> PostAsync(MusclePostDTO payload, CancellationToken ct);
    Task<APIResult<MusclePutDTO>> PutAsync(MuscleId id, MusclePutDTO payload, CancellationToken ct);
    Task<APIResult<MuscleUpdateOutcome>> PatchAsync(MuscleId id, MuscleUpdateDTO payload, CancellationToken ct);
    Task<APIResult<MuscleGetDTO>> DeleteAsync(MuscleId id, CancellationToken ct);
}