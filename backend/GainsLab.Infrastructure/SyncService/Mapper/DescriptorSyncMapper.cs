using GainsLab.Application.DTOs;
using GainsLab.Application.DTOs.Description;
using GainsLab.Contracts.Dtos.SyncDto;
using GainsLab.Infrastructure.Utilities;

namespace GainsLab.Contracts.SyncService.Mapper;

public static class DescriptorSyncMapper
{
    /// <summary>
    /// Maps an descriptor sync DTO into an EF Core DTO ready for persistence.
    /// </summary>
    /// <param name="dto">The upstream DTO representing the descriptor.</param>
    /// <param name="syncActor">Identifier used to stamp audit metadata.</param>
    /// <returns>An EF Core DTO representing the incoming descriptor.</returns>
    public static DescriptorRecord FromSyncDTO(DescriptorSyncDTO dto,string syncActor)
    {
        var entity = new DescriptorRecord();
        
        CoreUtilities.Logger.Log(nameof(DescriptorSyncMapper), $"mapping desc sync dto {dto.DescriptionContent} - {dto.GUID}");
        
        entity.Content = dto.DescriptionContent;
        entity.GUID = dto.GUID;
        entity.Authority = dto.Authority;
        
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
    public static DescriptorSyncDTO ToSyncDTO(DescriptorRecord e)
    {
        return new DescriptorSyncDTO(
            e.Iguid,
            e.Content,
            e.UpdatedAtUtc,
            e.UpdatedSeq,
            e.IsDeleted,
            e.Authority);
    }
}
