using GainsLab.Application.DTOs.Description;
using GainsLab.Application.Results;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.PostDto.Outcome;
using GainsLab.Contracts.Dtos.UpdateDto;
using GainsLab.Contracts.Dtos.UpdateDto.Outcome;
using GainsLab.Contracts.Dtos.UpdateDto.Request;

namespace GainsLab.Application.Interfaces.DataManagement.Gateway;

public interface IDescriptorGateway
{
    public Task<Result<IReadOnlyList<DescriptorGetDTO>>> GetAllDescriptorAsync();
    Task<Result<DescriptorUpdateOutcome>> UpdateDescriptorAsync(DescriptorUpdateRequest updateDescriptor,  ICache? cache);
    Task<Result<DescriptorCreateOutcome>> CreateDescriptorAsync(DescriptorPostDTO descriptorPostDto,  ICache? cache);
}