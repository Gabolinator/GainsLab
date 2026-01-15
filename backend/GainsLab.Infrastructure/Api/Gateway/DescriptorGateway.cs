using GainsLab.Application.DTOs.Description;
using GainsLab.Application.Interfaces.DataManagement;
using GainsLab.Application.Interfaces.DataManagement.Gateway;
using GainsLab.Application.Interfaces.DataManagement.Provider;
using GainsLab.Application.Results;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.PostDto.Outcome;
using GainsLab.Contracts.Dtos.SyncDto;
using GainsLab.Contracts.Dtos.UpdateDto;
using GainsLab.Contracts.Dtos.UpdateDto.Outcome;
using GainsLab.Contracts.Dtos.UpdateDto.Request;
using GainsLab.Contracts.SyncService.Mapper;
using GainsLab.Domain.Interfaces;
using GainsLab.Infrastructure.Caching.QueryCache;
using GainsLab.Infrastructure.SyncService;

namespace GainsLab.Infrastructure.Api.Gateway;

public class DescriptorGateway : IDescriptorGateway
{
    private readonly IDescriptorProvider _provider;
    private readonly ILogger _logger;

    public DescriptorGateway(IDescriptorProvider provider, ILogger logger)
    {
        _provider = provider;
        _logger = logger;
    }
    

    public async Task<Result<IReadOnlyList<DescriptorGetDTO>>> GetAllDescriptorAsync()
    {
        var syncDtos = await GetAllDescriptorSyncDtoAsync();
        if(!syncDtos.Success) return Result<IReadOnlyList<DescriptorGetDTO>>.Failure(syncDtos.GetErrorMessage());
        return Result<IReadOnlyList<DescriptorGetDTO>>.SuccessResult(syncDtos.Value != null
            ? syncDtos.Value.Select(s => DescriptorSyncMapper.ToGetDTO(s, "sync")).ToList(): new());
    }

    
    public async Task<Result<DescriptorUpdateOutcome>> UpdateDescriptorAsync(DescriptorUpdateRequest updateDescriptor,  ICache? cache)
    {
       var result = await _provider.UpdateDescriptorAsync(updateDescriptor, default);
       return result;
       if(result.Success) cache?.Invalidate();
       return result;
    }

    public async Task<Result<DescriptorCreateOutcome>> CreateDescriptorAsync(DescriptorPostDTO descriptorPostDto,
        ICache? cache)
    {
       var result = await _provider.CreateDescriptorAsync(descriptorPostDto, default);
       if(result.Success) cache?.Invalidate();
       return result;
    }


    public async Task<Result<IReadOnlyList<DescriptorSyncDTO>>> GetAllDescriptorSyncDtoAsync()
    {
        var result =  await _provider.PullDescriptorPageAsync(SyncCursorUtil.MinValue,200,default);
        if (!result.Success)
        {
            return Result<IReadOnlyList<DescriptorSyncDTO>>.Failure(result.GetErrorMessage());
        }
        
        return Result<IReadOnlyList<DescriptorSyncDTO>>.SuccessResult(result.Value != null ? 
            result.Value.ItemsList.Cast<DescriptorSyncDTO>().ToList():
            new());
    }
}