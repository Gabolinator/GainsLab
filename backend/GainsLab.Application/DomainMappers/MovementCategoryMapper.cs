using GainsLab.Application.DTOs;
using GainsLab.Application.DTOs.Description;
using GainsLab.Application.DTOs.MovementCategory;
using GainsLab.Contracts.Interface;
using GainsLab.Domain;
using GainsLab.Domain.Entities.CreationInfo;
using GainsLab.Domain.Entities.Descriptor;
using GainsLab.Domain.Entities.Identifier;
using GainsLab.Domain.Entities.WorkoutEntity;
using GainsLab.Domain.Interfaces;

namespace GainsLab.Application.DomainMappers;

/// <summary>
/// Converts movement category aggregates to and from their DTO counterparts.
/// </summary>
public static class MovementCategoryMapper
{
    /// <summary>
    /// Projects a movement category domain entity into a persistence DTO.
    /// </summary>
    public static MovementCategoryRecord? ToRecord(MovementCategoryEntity? domain, IClock clock)
    {
        if (domain == null) return null;

        return new MovementCategoryRecord
        {
            Id = domain.DbId > 0 ? domain.DbId : 0,
            GUID = domain.Id,
            Name = domain.Content.Name,
            DescriptorID = domain.Descriptor.DbId,
            Descriptor = (DescriptorRecord?)domain.Descriptor.ToRecord(clock),
            CreatedAtUtc = domain.CreationInfo.CreatedAtUtc,
            CreatedBy = domain.CreationInfo.CreatedBy,
            UpdatedAtUtc = domain.CreationInfo.UpdatedAtUtc ?? clock.UtcNow,
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
    public static MovementCategoryEntity? ToDomain(MovementCategoryRecord? dto)
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
    public static IEnumerable<MovementCategoryRelationRecord> CreateMovementCategoriesRelationDTOs(
        IReadOnlyCollection<MovementCategoryRecord> categoryDtos,
        IEnumerable<MovementCategoryEntity> categories)
    {
        if (categoryDtos == null) throw new ArgumentNullException(nameof(categoryDtos));
        if (categories == null) throw new ArgumentNullException(nameof(categories));

        var dtoByGuid = categoryDtos.ToDictionary(dto => dto.GUID);
        var dtoByName = categoryDtos
            .Where(dto => !string.IsNullOrWhiteSpace(dto.Name))
            .GroupBy(dto => dto.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var relations = new List<MovementCategoryRelationRecord>();

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
        MovementCategoryRecord parentDto,
        MovementCategoryRecord childDto,
        ICollection<MovementCategoryRelationRecord> buffer)
    {
        if (childDto.BaseCategoryLinks.Any(link => link.ParentCategory.GUID == parentDto.GUID))
        {
            return;
        }

        var relation = new MovementCategoryRelationRecord()
        {
            ParentCategory = parentDto,
            ChildCategory = childDto
        };

        parentDto.ChildCategoryLinks.Add(relation);
        childDto.BaseCategoryLinks.Add(relation);
        buffer.Add(relation);
    }

    private static IEnumerable<eMovementCategories> ExtractBaseCategories(MovementCategoryRecord dto)
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

    private static eMovementCategories[] InferFallbackBaseCategories(MovementCategoryRecord dto)
    {
        return Enum.TryParse(dto.Name, true, out eMovementCategories inferred) &&
               inferred != eMovementCategories.undefined
            ? new[] { inferred }
            : new[] { eMovementCategories.Hybrid };
    }
}
