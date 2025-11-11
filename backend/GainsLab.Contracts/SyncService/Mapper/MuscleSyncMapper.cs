using System.Collections.Generic;
using System.Linq;
using GainsLab.Contracts.SyncDto;
using GainsLab.Infrastructure.DB.DTOs;

namespace GainsLab.Contracts.SyncService.Mapper;

/// <summary>
/// Converts between muscle EF DTOs and wire sync DTOs.
/// </summary>
public static class MuscleSyncMapper
{
    /// <summary>
    /// Creates a EF DTO ready for persistence from a sync payload.
    /// </summary>
    public static MuscleDTO FromSyncDTO(MuscleSyncDTO dto, DescriptorDTO? descriptor, string syncActor)
    {
        return new MuscleDTO
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
    public static MuscleSyncDTO ToSyncDTO(MuscleDTO dto, IReadOnlyList<Guid>? antagonists = null)
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
}
