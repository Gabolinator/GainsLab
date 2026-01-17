using GainsLab.Application.DTOs;
using GainsLab.Application.DTOs.Description;
using GainsLab.Application.DTOs.MovementCategory;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.SyncDto;
using GainsLab.Domain;

namespace GainsLab.Infrastructure.SyncService.Mapper;

/// <summary>
/// Converts movement-category EF DTOs to/from the wire sync DTO.
/// </summary>
public static class MovementCategorySyncMapper
{
    /// <summary>
    /// Projects a sync payload into a tracked EF DTO. Relationship wiring (parent/base-category links)
    /// should be handled by the caller once the referenced categories are resolved.
    /// </summary>
    public static MovementCategoryRecord FromSyncDTO(
        MovementCategorySyncDTO dto,
        DescriptorRecord? descriptor,
        string syncActor)
    {
        return new MovementCategoryRecord
        {
            Name = dto.Name,
            GUID = dto.GUID,
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
    /// Projects an EF DTO and the denormalized relationship metadata to the sync contract.
    /// </summary>
    public static MovementCategorySyncDTO ToSyncDTO(
        MovementCategoryRecord dto,
        (Guid? parentCategoryGuid, IReadOnlyList<eMovementCategories>? baseCategories) parentAndBase)
    {
        return new MovementCategorySyncDTO(
            dto.GUID,
            dto.Name,
            dto.Descriptor?.GUID,
            parentAndBase.parentCategoryGuid,
            parentAndBase.baseCategories ?? Array.Empty<eMovementCategories>(),
            dto.UpdatedAtUtc,
            dto.UpdatedSeq,
            dto.IsDeleted,
            dto.Authority);
    }

    public static MovementCategoryRefDTO ToRefDto(this MovementCategorySyncDTO dto)
    {
        return new(dto.GUID, dto.Name);
    }

    public static async Task<MovementCategoryGetDTO?> ToGetDTOAsync(
        MovementCategorySyncDTO? dto, 
        Task<DescriptorGetDTO?> descriptor,
        DateTimeOffset createdAtUtc,
        string sync, 
        MovementCategoryRefDTO? parent,  
        IReadOnlyList< MovementCategoryRefDTO>? baseCategories,
        IReadOnlyList< MovementCategoryRefDTO>? childCategories)
    {
        if (dto == null) return null;
        var d = await descriptor;
      
        return new MovementCategoryGetDTO(
            dto.GUID,
            dto.Name,
            dto.DescriptorGUID,
            d,
            dto.ParentCategoryGUID,
            dto.BaseCategories,
            createdAtUtc,
            dto.UpdatedAtUtc,
            dto.UpdatedSeq,
            dto.IsDeleted,
            dto.Authority)
        {
            ChildCategories = childCategories ,
            ParentCategory = parent,
            BaseCategories = baseCategories ,
        };
    }
    
}
