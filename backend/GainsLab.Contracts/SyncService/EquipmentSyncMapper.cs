using GainsLab.Contracts.SyncDto;
using GainsLab.Infrastructure.DB.DTOs;
using GainsLab.Models.DataManagement.DB.Model.DTOs;

namespace GainsLab.Contracts.SyncService;

/// <summary>
/// Converts between sync DTOs and EF Core DTOs for equipment entities.
/// </summary>
public static class EquipmentSyncMapper
{
    /// <summary>
    /// Maps an equipment sync DTO into an EF Core DTO ready for persistence.
    /// </summary>
    /// <param name="dto">The upstream DTO representing the equipment.</param>
    /// <param name="descriptor">Optional descriptor DTO already tracked by EF Core.</param>
    /// <param name="syncActor">Identifier used to stamp audit metadata.</param>
    /// <returns>An EF Core DTO representing the incoming equipment.</returns>
    public static EquipmentDTO FromSyncDTO(EquipmentSyncDto dto, DescriptorDTO? descriptor ,string syncActor)
    {
        var entity = new EquipmentDTO();
        
        entity.Name = dto.Name;

        // Set the relationship; EF will handle DescriptorID
        entity.Descriptor = descriptor;

        entity.UpdatedAtUtc = dto.UpdatedAtUtc;
        entity.UpdatedSeq = dto.UpdatedSeq;
        entity.UpdatedBy = syncActor;
        entity.Version = dto.UpdatedSeq;
        entity.IsDeleted = dto.IsDeleted;
        entity.DeletedAt = dto.IsDeleted ? dto.UpdatedAtUtc : null;
        entity.DeletedBy = dto.IsDeleted ? syncActor : null;

        return entity;
    }
    
    /// <summary>
    /// Projects an EF Core DTO into a sync DTO for downstream consumers.
    /// </summary>
    /// <param name="e">The EF Core DTO retrieved from the database.</param>
    /// <returns>A sync DTO suitable for transmission to clients.</returns>
    public static EquipmentSyncDto ToSyncDTO(EquipmentDTO e)
    {
        return new EquipmentSyncDto(
            e.Iguid,
            e.Name,
            e.Descriptor == null ? null : e.Descriptor.GUID,
            e.UpdatedAtUtc,
            e.UpdatedSeq,
            e.IsDeleted);
    }
}
