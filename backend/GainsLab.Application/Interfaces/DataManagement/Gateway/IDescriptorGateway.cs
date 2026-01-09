using GainsLab.Application.DTOs.Description;
using GainsLab.Application.Results;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.PostDto.Outcome;
using GainsLab.Contracts.Dtos.UpdateDto;
using GainsLab.Contracts.Dtos.UpdateDto.Outcome;
using GainsLab.Contracts.Dtos.UpdateDto.Request;

namespace GainsLab.Application.Interfaces.DataManagement.Gateway;

public interface IDescriptorGateway
{
    public bool TryGetDescriptor(Guid descriptorGuid, out DescriptorRecord? descriptor);
    public Task<Result<DescriptorRecord>> TryGetDescriptorAsync(Guid? descriptorGuid);
    public Task<Result<IReadOnlyList<DescriptorRecord>>> GetAllDescriptorAsync();
    Task UpdateCacheAsync();
    Task<Result<DescriptorUpdateOutcome>> UpdateDescriptorAsync(DescriptorUpdateRequest updateDescriptor);
    Task<Result<DescriptorCreateOutcome>> CreateDescriptorAsync(DescriptorPostDTO descriptorPostDto);
}