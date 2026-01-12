using GainsLab.Application.DTOs.Description;
using GainsLab.Application.Results.APIResults;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.PutDto;
using GainsLab.Contracts.Dtos.UpdateDto;
using GainsLab.Contracts.Dtos.UpdateDto.Outcome;

namespace GainsLab.Application.Interfaces.DataManagement.Repository;


public interface IDescriptorRepository
{
    Task<APIResult<DescriptorGetDTO>> PullByIdAsync(Guid id, CancellationToken ct);
    Task<APIResult<DescriptorGetDTO>> PostAsync(DescriptorPostDTO payload, CancellationToken ct);
    Task<APIResult<DescriptorPutDTO>> PutAsync(Guid id, DescriptorPutDTO payload, CancellationToken ct);

    Task<APIResult<DescriptorUpdateOutcome>> PatchAsync(Guid id, DescriptorUpdateDTO payload, CancellationToken ct);

    public Task<APIResult<DescriptorRecord>> CreateAsync(DescriptorRecord entity, CancellationToken ct);
}