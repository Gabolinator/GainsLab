using System;
using System.Collections.Generic;
using System.Linq;
using GainsLab.Core.Models.Core;
using GainsLab.Core.Models.Core.CreationInfo;
using GainsLab.Core.Models.Core.Entities.Descriptor;
using GainsLab.Core.Models.Core.Entities.Identifier;
using GainsLab.Core.Models.Core.Entities.WorkoutEntity;
using GainsLab.Core.Models.Core.Interfaces.DB;
using GainsLab.Core.Models.Core.Utilities;
using GainsLab.Infrastructure.DB.DTOs;
using GainsLab.Models.Core;
using GainsLab.Models.DataManagement.DB.Model.DomainMappers;

namespace GainsLab.Infrastructure.DB.DomainMappers;

/// <summary>
/// Converts movement category aggregates to and from their DTO counterparts.
/// </summary>
public static class MovementCategoryMapper
{
    /// <summary>
    /// Projects a movement category domain entity into a persistence DTO.
    /// </summary>
    public static IDto? ToDTO(MovementCategoryEntity? domain)
    {
        if (domain == null) return null;

        return new MovementCategoryDTO
        {
            Id = domain.DbId > 0 ? domain.DbId : 0,
            GUID = domain.Id,
            Name = domain.Content.Name,
            DescriptorID = domain.Descriptor.DbId,
            Descriptor = (DescriptorDTO?)domain.Descriptor.ToDTO(),
            CreatedAtUtc = domain.CreationInfo.CreatedAtUtc,
            CreatedBy = domain.CreationInfo.CreatedBy,
            UpdatedAtUtc = domain.CreationInfo.UpdatedAtUtc ?? CoreUtilities.Clock.UtcNow,
            UpdatedBy = domain.CreationInfo.UpdatedBy,
            Version = domain.CreationInfo.Version,
            IsDeleted = domain.CreationInfo.IsDeleted,
            DeletedAt = domain.CreationInfo.DeletedAt,
            DeletedBy = domain.CreationInfo.DeletedBy
        };
    }

    /// <summary>
    /// Rebuilds a domain entity from a DTO retrieved from the database.
    /// </summary>
    public static MovementCategoryEntity? ToDomain(MovementCategoryDTO? dto)
    {
        if (dto == null) return null;

        var descriptor = (BaseDescriptorEntity?)dto.Descriptor?.ToDomain() ?? new BaseDescriptorEntity();

        var baseCategories = ExtractBaseCategories(dto).ToArray();
        if (baseCategories.Length == 0)
        {
            baseCategories = InferFallbackBaseCategories(dto);
        }

        var content = new MovementCategoryContent(dto.Name, baseCategories);
        content.ParentCategoryId = dto.ParentCategory != null
            ? new MovementCategoryId(dto.ParentCategory.GUID)
            : (MovementCategoryId?)null;

        var audit = new AuditedInfo(
            dto.CreatedAtUtc,
            dto.CreatedBy,
            dto.UpdatedAtUtc,
            dto.UpdatedBy,
            dto.Version,
            dto.IsDeleted,
            dto.DeletedAt,
            dto.DeletedBy);

        return new MovementCategoryEntity(
            content,
            new MovementCategoryId(dto.GUID),
            audit,
            descriptor,
            dto.Id);
    }

    /// <summary>
    /// Creates relation DTOs linking categories to their parents/base categories.
    /// </summary>
    public static IEnumerable<MovementCategoryRelationDTO> CreateMovementCategoriesRelationDTOs(
        IReadOnlyCollection<MovementCategoryDTO> categoryDtos,
        IEnumerable<MovementCategoryEntity> categories)
    {
        if (categoryDtos == null) throw new ArgumentNullException(nameof(categoryDtos));
        if (categories == null) throw new ArgumentNullException(nameof(categories));

        var dtoByGuid = categoryDtos.ToDictionary(dto => dto.GUID);
        var dtoByName = categoryDtos
            .Where(dto => !string.IsNullOrWhiteSpace(dto.Name))
            .GroupBy(dto => dto.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var relations = new List<MovementCategoryRelationDTO>();

        foreach (var category in categories)
        {
            if (category == null) continue;
            
            
            if (!dtoByGuid.TryGetValue(category.Id.Value, out var childDto)) continue;

            // Explicit parent linkage (user-defined hierarchy).
            if (category.Content.ParentCategoryId.HasValue &&
                dtoByGuid.TryGetValue(category.Content.ParentCategoryId.Value, out var parentDto))
            {
                childDto.ParentCategory = parentDto;
                childDto.ParentCategoryDbId = parentDto.Id > 0 ? parentDto.Id : childDto.ParentCategoryDbId;
                AttachRelation(parentDto, childDto, relations);
            }

            // Base enum categories.
            foreach (var baseEnum in category.Content.BaseCategories)
            {
                if (!dtoByName.TryGetValue(baseEnum.ToString(), out var baseDto)) continue;
                if (ReferenceEquals(baseDto, childDto)) continue;

                AttachRelation(baseDto, childDto, relations);
            }
        }

        return relations;
    }

    private static void AttachRelation(
        MovementCategoryDTO parentDto,
        MovementCategoryDTO childDto,
        ICollection<MovementCategoryRelationDTO> buffer)
    {
        if (childDto.BaseCategoryLinks.Any(link => link.ParentCategory.GUID == parentDto.GUID))
        {
            return;
        }

        var relation = new MovementCategoryRelationDTO
        {
            ParentCategory = parentDto,
            ChildCategory = childDto
        };

        parentDto.ChildCategoryLinks.Add(relation);
        childDto.BaseCategoryLinks.Add(relation);
        buffer.Add(relation);
    }

    private static IEnumerable<eMovementCategories> ExtractBaseCategories(MovementCategoryDTO dto)
    {
        foreach (var link in dto.BaseCategoryLinks)
        {
            if (link.ParentCategory == null) continue;
            if (Enum.TryParse(link.ParentCategory.Name, true, out eMovementCategories parsed) &&
                parsed != eMovementCategories.undefined)
            {
                yield return parsed;
            }
        }
    }

    private static eMovementCategories[] InferFallbackBaseCategories(MovementCategoryDTO dto)
    {
        return Enum.TryParse(dto.Name, true, out eMovementCategories inferred) &&
               inferred != eMovementCategories.undefined
            ? new[] { inferred }
            : new[] { eMovementCategories.Hybrid };
    }
}
