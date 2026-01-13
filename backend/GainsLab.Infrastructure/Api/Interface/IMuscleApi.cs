using GainsLab.Application.Results;
using GainsLab.Contracts.Dtos.Delete.Outcome;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.ID;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.PostDto.Outcome;
using GainsLab.Contracts.Dtos.UpdateDto.Outcome;
using GainsLab.Contracts.Dtos.UpdateDto.Request;
using GainsLab.Contracts.Interface;

namespace GainsLab.Infrastructure.Api.Interface;

public interface IMuscleApi
{
    Task<Result<ISyncPage<ISyncDto>>> PullMusclePageAsync(ISyncCursor cursor, int take, CancellationToken ct);
    
    Task<Result<MuscleGetDTO>> GetMuscleAsync(MuscleEntityId entity, CancellationToken ct);
    
    Task<Result<MuscleCreateOutcome>>  CreateMuscleAsync(MusclePostDTO entity, CancellationToken ct);
    
    Task<Result<MuscleUpdateOutcome>> UpdateMuscleAsync(MuscleUpdateRequest request, CancellationToken ct);
    
    Task<Result<MuscleDeleteOutcome>> DeleteMuscleAsync(MuscleEntityId entity, CancellationToken ct);
}