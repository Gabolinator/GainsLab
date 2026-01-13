using GainsLab.Application.Interfaces.DataManagement.Repository;
using GainsLab.Application.Results.APIResults;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.PutDto;
using GainsLab.Contracts.Dtos.UpdateDto;
using GainsLab.Contracts.Dtos.UpdateDto.Outcome;

namespace GainsLab.Infrastructure.DB.Repository;

public class MovementCategoryRepository : IMovementCategoryRepository
{
    public Task<APIResult<MovementCategoryGetDTO>> PullByIdAsync(Guid id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<APIResult<MovementCategoryGetDTO>> PostAsync(MovementCategoryPostDTO payload, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<APIResult<MovementCategoryPutDTO>> PutAsync(Guid id, MovementCategoryPutDTO payload, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<APIResult<MovementCategoryUpdateOutcome>> PatchAsync(Guid id, MovementCategoryUpdateDTO payload, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<APIResult<MovementCategoryGetDTO>> DeleteAsync(Guid id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}