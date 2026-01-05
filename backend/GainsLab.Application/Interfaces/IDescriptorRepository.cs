using GainsLab.Application.DTOs;
using GainsLab.Application.Results.APIResults;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.PutDto;
using GainsLab.Contracts.Dtos.UpdateDto;

namespace GainsLab.Application.Interfaces;


public interface IDescriptorRepository
{
    Task<APIResult<DescriptorGetDTO>> PullByIdAsync(Guid id, CancellationToken ct);
    Task<APIResult<DescriptorGetDTO>> PostAsync(DescriptorPostDTO payload, CancellationToken ct);
    Task<APIResult<DescriptorPutDTO>> PutAsync(Guid id, DescriptorPutDTO payload, CancellationToken ct);

    Task<APIResult<DescriptorUpdateDTO>> PatchAsync(Guid id, DescriptorUpdateDTO payload, CancellationToken ct);
}