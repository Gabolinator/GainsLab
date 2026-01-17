using GainsLab.Application.DTOs;
using GainsLab.Application.DTOs.Description;
using GainsLab.Application.DTOs.Muscle;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.SyncDto;

namespace GainsLab.Infrastructure.SyncService.Mapper;

/// <summary>
/// Converts between muscle EF DTOs and wire sync DTOs.
/// </summary>
public static class MuscleSyncMapper
{
    /// <summary>
    /// Creates a EF DTO ready for persistence from a sync payload.
    /// </summary>
    public static MuscleRecord FromSyncDTO(MuscleSyncDTO dto, DescriptorRecord? descriptor, string syncActor)
    {
        return new MuscleRecord
        {
            Name = dto.Name,
            GUID = dto.GUID,
            BodySection = dto.BodySection,
            Descriptor = descriptor,
            Authority = dto.Authority,
            UpdatedAtUtc = dto.UpdatedAtUtc,
            UpdatedSeq = dto.UpdatedSeq,
            UpdatedBy = syncActor,
            Version = dto.UpdatedSeq,
            IsDeleted = dto.IsDeleted,
            DeletedAt = dto.IsDeleted ? dto.UpdatedAtUtc : null,
            DeletedBy = dto.IsDeleted ? syncActor : null
        };
    }

    /// <summary>
    /// Projects a tracked EF DTO to the sync contract, injecting antagonist GUIDs when supplied.
    /// </summary>
    public static MuscleSyncDTO ToSyncDTO(MuscleRecord dto, IReadOnlyList<Guid>? antagonists = null)
    {
        return new MuscleSyncDTO(
            dto.GUID,
            dto.Name,
            dto.Descriptor?.GUID,
            dto.BodySection,
            antagonists ?? dto.AntagonistGUIDs.ToList(),
            dto.UpdatedAtUtc,
            dto.UpdatedSeq,
            dto.IsDeleted,
            dto.Authority);
    }

    public static MuscleRefDTO ToRefDto(this MuscleSyncDTO syncDto)
    {
            return new MuscleRefDTO(syncDto.GUID, syncDto.Name);
    }

    
    
    public static async Task<MuscleGetDTO> ToGetDTOAsync(MuscleSyncDTO syncDto, Task<DescriptorGetDTO?> getDescriptorAsync, IReadOnlyList<MuscleRefDTO>? antagonist, DateTimeOffset updatedAtUtc, string sync)
    {
        var descriptor = await getDescriptorAsync;
        
        //get the antagonists 
        return new MuscleGetDTO
            (
                syncDto.GUID,
                syncDto.Name,
                "",
                syncDto.BodySection,
                syncDto.DescriptorGUID,
                descriptor,
                syncDto.AntagonistGuids,
                antagonist,
                updatedAtUtc,
                syncDto.UpdatedAtUtc,
                syncDto.UpdatedSeq,
                syncDto.IsDeleted,
                syncDto.Authority);

    }
}
