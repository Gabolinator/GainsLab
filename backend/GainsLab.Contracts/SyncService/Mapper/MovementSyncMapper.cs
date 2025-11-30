using GainsLab.Contracts.SyncDto;
using GainsLab.Infrastructure.DB.DTOs;

namespace GainsLab.Contracts.SyncService.Mapper;

public static class MovementSyncMapper
{
    public static MovementDTO FromSyncDTO(MovementSyncDTO dto, DescriptorDTO? descriptor, string syncActor)
    {

        return new MovementDTO
        {
            Name = dto.Name,
            GUID = dto.GUID,
            Descriptor = descriptor,
            DescriptorID = descriptor?.Id ?? 0,
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


    public static MovementSyncDTO ToSyncDTO(
        MovementDTO dto, 
        IReadOnlyList<Guid>? Equipment = null,
        IReadOnlyList<Guid>? PrimaryMuscles = null, 
        IReadOnlyList<Guid>? SecondaryMuscles = null, 
        Guid? category = null,
        Guid? variantOf = null)
    {

        return new MovementSyncDTO(
            dto.GUID,
            dto.Name,
            dto.Descriptor?.GUID,
            PrimaryMuscles ?? dto.PrimaryMuscleGUIDs.ToList(),
            SecondaryMuscles ?? dto.SecondaryMuscleGUIDs.ToList(),
            Equipment ?? dto.EquipmentGUIDs.ToList(),
            category ?? (dto.Category?.GUID ?? Guid.Empty),
            variantOf ?? dto.VariantOfMovementGuid,
            dto.UpdatedAtUtc,
            dto.UpdatedSeq,
            dto.IsDeleted,
            dto.Authority);

    }
}
