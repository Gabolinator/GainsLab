using System.Linq;
using GainsLab.Contracts.SyncDto;
using GainsLab.Core.Models.Core;
using GainsLab.Infrastructure.DB.DTOs;

namespace GainsLab.Contracts.SyncService;

/// <summary>
/// Converts between muscle sync DTOs and EF Core DTOs.
/// </summary>
public static class MuscleSyncMapper
{
    public static MuscleDTO FromSyncDTO(MuscleSyncDTO dto, DescriptorDTO? descriptor, string syncActor)
    {
        var entity = new MuscleDTO
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

        return entity;
    }

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
