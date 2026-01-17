using System.Security.Cryptography.Xml;
using GainsLab.Application.DTOs.Muscle;
using GainsLab.Application.Interfaces.DataManagement.Gateway;
using GainsLab.Application.Interfaces.DataManagement.Provider;
using GainsLab.Application.Results;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.SyncDto;
using GainsLab.Domain.Interfaces;
using GainsLab.Infrastructure.Caching.Registry;
using GainsLab.Infrastructure.SyncService;
using GainsLab.Infrastructure.SyncService.Mapper;

namespace GainsLab.Infrastructure.Api.Gateway;

public class MuscleGateway : IMuscleGateway
{
    private readonly IMuscleProvider _provider;
    private readonly ILogger _logger;
    private readonly DescriptorRegistry _descriptorGateway;
    
    public  MuscleGateway(IMuscleProvider provider, DescriptorRegistry descriptorGateway ,ILogger logger)
    {
        _provider = provider;
        _descriptorGateway = descriptorGateway;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<MuscleGetDTO>>> GetAllMusclesAsync()
    {
        var syncDtos = await GetAllMuscleSyncDtoAsync();

        if (!syncDtos.Success)
            return Result<IReadOnlyList<MuscleGetDTO>>
                .Failure(syncDtos.GetErrorMessage());

        if (syncDtos.Value == null)
            return Result<IReadOnlyList<MuscleGetDTO>>
                .SuccessResult(Array.Empty<MuscleGetDTO>());

       
        var refs = syncDtos.Value!.ToDictionary(x => x.GUID, x=> x.ToRefDto());
        
        
        
        var tasks = syncDtos.Value.Select(s =>
           MuscleSyncMapper.ToGetDTOAsync(
                s,
                GetDescriptorAsync(s.DescriptorGUID),
                GetAntagonistRefs(refs,s.AntagonistGuids),
                s.UpdatedAtUtc,
                "sync"
            )
        );

        var dtos = await Task.WhenAll(tasks);
        
        return Result<IReadOnlyList<MuscleGetDTO>>
            .SuccessResult(dtos!);
        
    }

    IReadOnlyList<MuscleRefDTO>? GetAntagonistRefs(Dictionary<Guid, MuscleRefDTO> dict, IReadOnlyList<Guid>? antonistGuids)
    {
        if(antonistGuids == null || !antonistGuids.Any() || !dict.Any()) return null;
        
        
        MuscleRefDTO GetRef(Guid guid)
        {
           return dict.TryGetValue(guid, out var result) ? result :new MuscleRefDTO(guid, "");
        }
        
        return antonistGuids.Select(GetRef).ToList();
        
    }

   

    private Task<DescriptorGetDTO?> GetDescriptorAsync(Guid? id)
        => _descriptorGateway.GetDescriptorByIdAsync(id);


    public async Task<Result<IReadOnlyList<MuscleSyncDTO>>>GetAllMuscleSyncDtoAsync()
    {
        var result =  await _provider.PullMusclePageAsync(SyncCursorUtil.MinValue, 200, default);;
        if (!result.Success)
        {
            return Result<IReadOnlyList<MuscleSyncDTO>>.Failure(result.GetErrorMessage());
        }
        
        return Result<IReadOnlyList<MuscleSyncDTO>>.SuccessResult(result.Value != null ? 
            result.Value.ItemsList.Cast<MuscleSyncDTO>().ToList():
            new());

    }

    
}