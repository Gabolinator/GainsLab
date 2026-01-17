using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using GainsLab.Application.DTOs;
using GainsLab.Application.DTOs.Description;
using GainsLab.Application.DTOs.MovementCategory;
using GainsLab.Contracts;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.PutDto;
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
    public static MovementCategoryGetDTO? ToGetDTO(this MovementCategoryRecord? record) =>
        record.TryMapToGetDTO(out var dto) ? dto : null;

    public static bool TryMapToGetDTO(this MovementCategoryRecord? record,out MovementCategoryGetDTO? dto)
    {
        if (record == null)
        {
            dto = null;
            return false;
        }

        
        
     //   var baseCategories = ExtractCategoriesFromLinks(record.BaseCategoryLinks, allRecords).ToArray();
        //
        // var  bases = baseCategories.Select(c=>ToGetDTO(c.parent,allRecords)).Where(c=>c !=null)!;
        //
        // var childCategories = ExtractCategoriesFromLinks(record.ChildCategoryLinks, allRecords).ToArray();
        // IEnumerable<MovementCategoryGetDTO> childs = childCategories.Select(c=>ToGetDTO(c.child,allRecords)).Where(c=>c !=null)!;
        //

        dto = new MovementCategoryGetDTO(
            record.GUID,
            record.Name,
            record.Descriptor?.GUID,
            record.Descriptor.ToGetDTO(),
            record.ParentCategory?.GUID,
            ExtractCategoriesFromLinks(record.BaseCategoryLinks),
            record.CreatedAtUtc,
            record.UpdatedAtUtc,
            record.UpdatedSeq,
            record.IsDeleted,
            record.Authority)
            {
                ParentCategory = record.ParentCategory.ToRefDTO()
            };
        
        return true;
    }

    public static MovementCategoryRefDTO? ToRefDTO(this MovementCategoryRecord? record)
    {
        if(record == null) return null;
        
        return new MovementCategoryRefDTO(record.GUID, record.Name);
    }

    private static IReadOnlyList<eMovementCategories>? ExtractCategoriesFromLinks(ICollection<MovementCategoryRelationRecord> links)
    {
        if (!links.Any()) return null;
        
        return links
            .Select(l=> Enum.TryParse(l.ParentCategory.Name, out eMovementCategories cat)? cat:eMovementCategories.undefined )
            .Where(c=>c!=eMovementCategories.undefined).ToList();

    }

    public static MovementCategoryRecord? ToEntity(
        this MovementCategoryPostDTO? dto,
        IClock clock,
        MovementCategoryRecord? parentCategory = null,
        IEnumerable<MovementCategoryRecord>? baseCategories = null,
        IEnumerable<MovementCategoryRecord>? childCategories = null) =>
        dto.TryMapToEntity(clock, out var record, parentCategory, baseCategories, childCategories) ? record : null;

    public static bool TryMapToEntity(
        this MovementCategoryPostDTO? dto,
        IClock clock,
        out MovementCategoryRecord? record,
        MovementCategoryRecord? parentCategory = null,
        IEnumerable<MovementCategoryRecord>? baseCategories = null,
        IEnumerable<MovementCategoryRecord>? childCategories = null)
    {
        if (dto == null)
        {
            record = null;
            return false;
        }

        dto.Descriptor.TryMapToEntity(clock, out var descriptorRecord);

        var newRecord = new MovementCategoryRecord
        {
            GUID = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
            Name = dto.Name,
            DescriptorID = descriptorRecord?.Id ?? 0,
            Descriptor = descriptorRecord,
            CreatedAtUtc = clock.UtcNow,
            CreatedBy = string.IsNullOrWhiteSpace(dto.CreatedBy) ? "system" : dto.CreatedBy!,
            UpdatedAtUtc = clock.UtcNow,
            UpdatedBy = dto.CreatedBy,
            UpdatedSeq = 1,
            Authority = dto.Authority,
            ParentCategory = parentCategory,
            ParentCategoryDbId = parentCategory?.Id
        };

        newRecord.BaseCategoryLinks = BuildBaseRelations(newRecord, baseCategories);
        newRecord.ChildCategoryLinks = BuildChildRelations(newRecord, childCategories);

        record = newRecord;
        return true;
    }

    public static MovementCategoryPutDTO? ToPutDTO(this MovementCategoryRecord? record, IClock clock, UpsertOutcome outcome) =>
        record.TryMapToPutDTO(clock, outcome, out var dto) ? dto : null;

    public static bool TryMapToPutDTO(this MovementCategoryRecord? record, IClock clock, UpsertOutcome outcome, out MovementCategoryPutDTO? dto)
    {
        if (record == null)
        {
            dto = null;
            return false;
        }

        var baseCategories = ExtractBaseCategories(record).ToArray();
        if (baseCategories.Length == 0)
        {
            baseCategories = InferFallbackBaseCategories(record);
        }

        record.Descriptor.TryMapToPutDTO(clock, outcome, out var descriptorPutDto);

        dto = new MovementCategoryPutDTO
        {
            Id = record.GUID,
            Outcome = outcome,
            Name = record.Name,
            ParentCategoryId = record.ParentCategory?.GUID,
            BaseCategories = baseCategories,
            Descriptor = descriptorPutDto ?? new DescriptorPutDTO
            {
                Id = record.Descriptor?.GUID,
                DescriptionContent = record.Descriptor?.Content ?? string.Empty,
                Authority = record.Authority,
                Outcome = outcome
            },
            Authority = record.Authority,
            UpdatedBy = record.UpdatedBy
        };

        return true;
    }

    private static IEnumerable<eMovementCategories> ExtractBaseCategories(MovementCategoryRecord record)
    {
        foreach (var link in record.BaseCategoryLinks)
        {
            if(link.ParentCategory == null) continue;
            if (Enum.TryParse<eMovementCategories>(link.ParentCategory.Name, out eMovementCategories parsed) &&
                parsed != eMovementCategories.undefined)
            {
                yield return parsed;
            }
        }
    }


    public static MovementCategoryRecord? ToEntity(
        this MovementCategoryPutDTO? dto,
        IClock clock,
        Guid? id = null,
        MovementCategoryRecord? existing = null,
        MovementCategoryRecord? parentCategory = null,
        IEnumerable<MovementCategoryRecord>? baseCategories = null,
        IEnumerable<MovementCategoryRecord>? childCategories = null) =>
        dto.TryMapToEntity(clock, out var mapped, id, existing, parentCategory, baseCategories, childCategories)
            ? mapped
            : null;

    public static bool TryMapToEntity(
        this MovementCategoryPutDTO? dto,
        IClock clock,
        out MovementCategoryRecord? record,
        Guid? id = null,
        MovementCategoryRecord? existing = null,
        MovementCategoryRecord? parentCategory = null,
        IEnumerable<MovementCategoryRecord>? baseCategories = null,
        IEnumerable<MovementCategoryRecord>? childCategories = null)
    {
        if (dto == null)
        {
            record = null;
            return false;
        }

        dto.Descriptor.TryMapToEntity(clock, out var descriptorRecord);
        var descriptor = descriptorRecord ?? existing?.Descriptor;

        var resolvedParent = parentCategory ?? existing?.ParentCategory;
        var resolvedParentDbId = parentCategory?.Id ?? existing?.ParentCategoryDbId;

        var recordCandidate = new MovementCategoryRecord
        {
            Id = existing?.Id ?? 0,
            GUID = ResolveMovementCategoryGuid(id, dto.Id, existing?.GUID),
            Name = dto.Name,
            DescriptorID = descriptor?.Id ?? existing?.DescriptorID ?? 0,
            Descriptor = descriptor,
            CreatedAtUtc = existing?.CreatedAtUtc ?? clock.UtcNow,
            CreatedBy = existing?.CreatedBy ?? (dto.UpdatedBy ?? "system"),
            UpdatedAtUtc = clock.UtcNow,
            UpdatedBy = dto.UpdatedBy,
            UpdatedSeq = existing?.UpdatedSeq ?? 1,
            Authority = dto.Authority,
            RowVersion = existing?.RowVersion,
            Version = existing?.Version ?? 0,
            IsDeleted = existing?.IsDeleted ?? false,
            DeletedAt = existing?.DeletedAt,
            DeletedBy = existing?.DeletedBy,
            ParentCategoryDbId = resolvedParentDbId,
            ParentCategory = resolvedParent,
            BaseCategoryLinks = new List<MovementCategoryRelationRecord>(),
            ChildCategoryLinks = new List<MovementCategoryRelationRecord>()
        };

        recordCandidate.BaseCategoryLinks = baseCategories != null
            ? BuildBaseRelations(recordCandidate, baseCategories)
            : (existing?.BaseCategoryLinks != null
                ? CloneBaseRelations(recordCandidate, existing.BaseCategoryLinks)
                : new List<MovementCategoryRelationRecord>());

        recordCandidate.ChildCategoryLinks = childCategories != null
            ? BuildChildRelations(recordCandidate, childCategories)
            : (existing?.ChildCategoryLinks != null
                ? CloneChildRelations(recordCandidate, existing.ChildCategoryLinks)
                : new List<MovementCategoryRelationRecord>());

        record = recordCandidate;
        return true;
    }

    /// <summary>
    /// Projects a movement category domain entity into a persistence DTO.
    /// </summary>
    public static MovementCategoryRecord? ToRecord(MovementCategoryEntity? domain, IClock clock) =>
        domain.TryMapToRecord(clock, out var record) ? record : null;

    public static bool TryMapToRecord(this MovementCategoryEntity? domain, IClock clock, out MovementCategoryRecord? record)
    {
        if (domain == null)
        {
            record = null;
            return false;
        }

        domain.Descriptor.TryMapToRecord(clock, out var descriptorRecord);

        record = new MovementCategoryRecord
        {
            Id = domain.DbId > 0 ? domain.DbId : 0,
            GUID = domain.Id,
            Name = domain.Content.Name,
            DescriptorID = descriptorRecord?.Id ?? 0,
            Descriptor = descriptorRecord,
            CreatedAtUtc = domain.CreationInfo.CreatedAtUtc,
            CreatedBy = domain.CreationInfo.CreatedBy,
            UpdatedAtUtc = domain.CreationInfo.UpdatedAtUtc ?? clock.UtcNow,
            UpdatedBy = domain.CreationInfo.UpdatedBy,
            Version = domain.CreationInfo.Version,
            IsDeleted = domain.CreationInfo.IsDeleted,
            DeletedAt = domain.CreationInfo.DeletedAt,
            DeletedBy = domain.CreationInfo.DeletedBy
        };

        return true;
    }

    /// <summary>
    /// Rebuilds a domain entity from a DTO retrieved from the database.
    /// </summary>
    public static MovementCategoryEntity? ToDomain(MovementCategoryRecord? dto) =>
        dto.TryMapToDomain(out var entity) ? entity : null;

    public static bool TryMapToDomain(this MovementCategoryRecord? dto, out MovementCategoryEntity? entity)
    {
        if (dto == null)
        {
            entity = null;
            return false;
        }

        dto.Descriptor.TryMapToDomain(out var descriptor);
        descriptor ??= new BaseDescriptorEntity();

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

        entity = new MovementCategoryEntity(
            content,
            new MovementCategoryId(dto.GUID),
            audit,
            descriptor,
            dto.Id);

        return true;
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
        if (childDto.BaseCategoryLinks != null &&
            childDto.BaseCategoryLinks.Any(link => link.ParentCategory?.GUID == parentDto.GUID))
        {
            return;
        }

        childDto.BaseCategoryLinks ??= new List<MovementCategoryRelationRecord>();
        parentDto.ChildCategoryLinks ??= new List<MovementCategoryRelationRecord>();

        var relation = new MovementCategoryRelationRecord()
        {
            ParentCategory = parentDto,
            ChildCategory = childDto
        };

        parentDto.ChildCategoryLinks.Add(relation);
        childDto.BaseCategoryLinks.Add(relation);
        buffer.Add(relation);
    }

    // private static IEnumerable<(MovementCategoryRecord? parent, MovementCategoryRecord? child)>  ExtractCategoriesFromLinks( IEnumerable<MovementCategoryRelationRecord>? links, IEnumerable<MovementCategoryRecord> allRecords)
    // {
    //     if (links == null || !links.Any() || !allRecords.Any())
    //     {
    //         return  Enumerable.Empty<(MovementCategoryRecord, MovementCategoryRecord)>();
    //     }
    //
    //     var list= new List<(MovementCategoryRecord? parent, MovementCategoryRecord? child)>(); 
    //     foreach (var link in links)
    //     {
    //      var parent = allRecords.FirstOrDefault(r=> r.Id == link.ParentCategoryId);
    //      var child = allRecords.FirstOrDefault(r=> r.Id == link.ChildCategoryId);
    //      list.Add((parent, child));
    //     }
    //     
    //     return list;
    // }

    private static eMovementCategories[] InferFallbackBaseCategories(MovementCategoryRecord dto)
    {
        return Enum.TryParse(dto.Name, true, out eMovementCategories inferred) &&
               inferred != eMovementCategories.undefined
            ? new[] { inferred }
            : new[] { eMovementCategories.Hybrid };
    }

    private static ICollection<MovementCategoryRelationRecord> BuildBaseRelations(
        MovementCategoryRecord child,
        IEnumerable<MovementCategoryRecord>? parents)
    {
        if (child == null || parents == null)
        {
            return new List<MovementCategoryRelationRecord>();
        }

        var relations = new List<MovementCategoryRelationRecord>();
        foreach (var parent in parents.Where(p => p != null).DistinctBy(p => p.GUID))
        {
            var relation = new MovementCategoryRelationRecord
            {
                ParentCategory = parent,
                ParentCategoryId = parent.Id,
                ChildCategory = child,
                ChildCategoryId = child.Id
            };
            relations.Add(relation);
        }

        return relations;
    }

    private static ICollection<MovementCategoryRelationRecord> BuildChildRelations(
        MovementCategoryRecord parent,
        IEnumerable<MovementCategoryRecord>? childs)
    {
        if (parent == null || childs == null)
        {
            return new List<MovementCategoryRelationRecord>();
        }

        var relations = new List<MovementCategoryRelationRecord>();
        foreach (var child in childs.Where(c => c != null).DistinctBy(c => c.GUID))
        {
            var relation = new MovementCategoryRelationRecord
            {
                ParentCategory = parent,
                ParentCategoryId = parent.Id,
                ChildCategory = child,
                ChildCategoryId = child.Id
            };
            relations.Add(relation);
        }

        return relations;
    }

    private static ICollection<MovementCategoryRelationRecord> CloneBaseRelations(
        MovementCategoryRecord child,
        IEnumerable<MovementCategoryRelationRecord> existingRelations)
    {
        if (existingRelations == null)
        {
            return new List<MovementCategoryRelationRecord>();
        }

        var parents = existingRelations
            .Where(rel => rel.ParentCategory != null)
            .Select(rel => rel.ParentCategory!)
            .ToList();

        return BuildBaseRelations(child, parents);
    }

    private static ICollection<MovementCategoryRelationRecord> CloneChildRelations(
        MovementCategoryRecord parent,
        IEnumerable<MovementCategoryRelationRecord> existingRelations)
    {
        if (existingRelations == null)
        {
            return new List<MovementCategoryRelationRecord>();
        }

        var childs = existingRelations
            .Where(rel => rel.ChildCategory != null)
            .Select(rel => rel.ChildCategory!)
            .ToList();

        return BuildChildRelations(parent, childs);
    }

    private static Guid ResolveMovementCategoryGuid(Guid? overrideId, Guid? dtoId, Guid? existingId)
    {
        if (overrideId.HasValue && overrideId.Value != Guid.Empty) return overrideId.Value;
        if (dtoId.HasValue && dtoId.Value != Guid.Empty) return dtoId.Value;
        if (existingId.HasValue && existingId.Value != Guid.Empty) return existingId.Value;
        return Guid.NewGuid();
    }
}
