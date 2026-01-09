using GainsLab.Application.Results;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.PostDto.Outcome;
using GainsLab.Contracts.Dtos.UpdateDto;
using GainsLab.Contracts.Dtos.UpdateDto.Outcome;
using GainsLab.Contracts.Dtos.UpdateDto.Request;
using GainsLab.Contracts.Interface;


namespace GainsLab.Infrastructure.Api.Interface;

public interface IDescriptorApi
{
    Task<Result<ISyncPage<ISyncDto>>> PullDescriptorPageAsync(ISyncCursor cursor, int take, CancellationToken ct);
    
    Task<Result<DescriptorGetDTO>> GetDescriptorAsync(DescriptorGetDTO entity, CancellationToken ct);
    
    Task<Result<DescriptorCreateOutcome>>  CreateDescriptorAsync(DescriptorPostDTO entity, CancellationToken ct);
    
    Task<Result<DescriptorUpdateOutcome>> UpdateDescriptorAsync(DescriptorUpdateRequest request, CancellationToken ct);
    
    //Task<Result<DescriptorGetDTO>> DeleteDescriptorAsync(DescriptorGetDTO entity, CancellationToken ct);
    
}