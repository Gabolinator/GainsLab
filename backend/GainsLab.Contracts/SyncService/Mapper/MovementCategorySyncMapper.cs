using System;
using System.Collections.Generic;
using GainsLab.Contracts.SyncDto;
using GainsLab.Core.Models.Core;
using GainsLab.Infrastructure.DB.DTOs;

namespace GainsLab.Contracts.SyncService.Mapper;

/// <summary>
/// Converts movement-category EF DTOs to/from the wire sync DTO.
/// </summary>
public static class MovementCategorySyncMapper
{
    /// <summary>
    /// Projects a sync payload into a tracked EF DTO. Relationship wiring (parent/base-category links)
    /// should be handled by the caller once the referenced categories are resolved.
    /// </summary>
    public static MovementCategoryDTO FromSyncDTO(
        MovementCategorySyncDto dto,
        DescriptorDTO? descriptor,
        string syncActor)
    {
        return new MovementCategoryDTO
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
    public static MovementCategorySyncDto ToSyncDTO(
        MovementCategoryDTO dto,
        Guid? parentCategoryGuid,
        IReadOnlyList<eMovementCategories>? baseCategories = null)
    {
        return new MovementCategorySyncDto(
            dto.GUID,
            dto.Name,
            dto.Descriptor?.GUID,
            parentCategoryGuid,
            baseCategories ?? Array.Empty<eMovementCategories>(),
            dto.UpdatedAtUtc,
            dto.UpdatedSeq,
            dto.IsDeleted,
            dto.Authority);
    }
}
