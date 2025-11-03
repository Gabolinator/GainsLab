using GainsLab.Contracts.SyncDto;
using GainsLab.Infrastructure.DB.DTOs;
using GainsLab.Models.DataManagement.DB.Model.DTOs;

namespace GainsLab.Contracts.SyncService;

public static class EquipmentSyncMapper
{
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