using System;
using System.Collections.Generic;
using System.Linq;
using GainsLab.Application.DTOs.MovementCategory;
using GainsLab.Contracts.Dtos.PutDto;
using GainsLab.Contracts.Dtos.UpdateDto;
using GainsLab.Domain.Interfaces;

namespace GainsLab.Application.DTOs.Extensions;

public static class MovementCategoryUpdateExtension
{
    public static bool AnythingChanged(this MovementCategoryRecord record, MovementCategoryPutDTO dto, IEnumerable<Guid>? bases = null, IEnumerable<Guid>? childs = null)
    {
        //can add other criterias as needed 
        return !string.Equals(
                   record.GetContent(), 
                   dto.Name, 
                   StringComparison.InvariantCultureIgnoreCase) 
               || record.Authority !=  dto.Authority 
               || ParentChanged(record, dto.ParentCategoryId)
               || ListChanged(record.BaseCategoryGUIDs, bases)
               || ListChanged(record.ChildGUIDs , childs);

       
    }

    private static bool ListChanged(IEnumerable<Guid>? list1, IEnumerable<Guid>? list2)
    {
        //list one is empty , is list 2 ?
        if (list1 == null || !list1.Any())
        {
            return list2 != null && list2.Any();
        }
        
        //list 1 was not empty but list 2 is
        if (list2 == null || !list2.Any()) return true;

        //compare per element
        return list1.Equals(list2);
    }


    public static bool AnythingChanged(this MovementCategoryRecord record, MovementCategoryUpdateDTO dto, IEnumerable<Guid>? bases = null, IEnumerable<Guid>? childs = null)
    {
        //can add other criterias as needed 
        return !string.Equals(
                   record.GetContent(),
                   dto.Name,
                   StringComparison.InvariantCultureIgnoreCase)
               || record.Authority != dto.Authority
               || ParentChanged(record, dto.ParentCategoryId)
               || ListChanged(record.BaseCategoryGUIDs, bases)
               || ListChanged(record.ChildGUIDs , childs);
    }
    
    
    public static bool TryUpdate(this MovementCategoryRecord record, MovementCategoryUpdateDTO dto, IClock clock,
        MovementCategoryRecord? parentCategory = null,
        ICollection<MovementCategoryRelationRecord>? baseCategoryRelations = null)
    {
        bool anyUpdate = false;
   
        if (!string.IsNullOrWhiteSpace(dto.Name) &&
            !string.Equals(record.Name, dto.Name, StringComparison.InvariantCultureIgnoreCase))
        {
            record.Name = dto.Name;
            anyUpdate = true;
        }
        
        if (dto.Authority != null && record.Authority !=  dto.Authority)
        {
            record.Authority = dto.Authority.Value;
            anyUpdate = true;
        }

        if (dto.ParentCategoryId.HasValue)
        {
            if (dto.ParentCategoryId.Value == Guid.Empty)
            {
                if (record.ParentCategory != null || record.ParentCategoryDbId.HasValue)
                {
                    record.ParentCategory = null;
                    record.ParentCategoryDbId = null;
                    anyUpdate = true;
                }
            }
            else if (parentCategory != null &&
                     (record.ParentCategory == null || record.ParentCategory.GUID != parentCategory.GUID))
            {
                record.ParentCategory = parentCategory;
                record.ParentCategoryDbId = parentCategory.Id;
                anyUpdate = true;
            }
        }

        if (baseCategoryRelations != null)
        {
            var existingBaseGuids = record.BaseCategoryLinks?
                                        .Select(link => link.ParentCategory?.GUID)
                                        .Where(guid => guid.HasValue && guid.Value != Guid.Empty)
                                        .Select(guid => guid!.Value)
                                        .OrderBy(guid => guid)
                                        .ToArray()
                                    ?? Array.Empty<Guid>();

            var incomingBaseGuids = baseCategoryRelations
                .Select(link => link.ParentCategory?.GUID)
                .Where(guid => guid.HasValue && guid.Value != Guid.Empty)
                .Select(guid => guid!.Value)
                .OrderBy(guid => guid)
                .ToArray();

            if (!existingBaseGuids.SequenceEqual(incomingBaseGuids))
            {
                record.BaseCategoryLinks = baseCategoryRelations;
                anyUpdate = true;
            }
        }
        
        if(!anyUpdate) return false;
        
        record.UpdatedAtUtc = clock.UtcNow;
        record.UpdatedBy = dto.UpdatedBy;
        
        return true;
    }
    
    



    public static bool ParentChanged(MovementCategoryRecord movementCategoryRecord,
        Guid? movementCategoryPutDto)
    {
        if (movementCategoryRecord.ParentCategory == null)
        {
            return movementCategoryPutDto !=null && movementCategoryPutDto != Guid.Empty;
        }
        
        if(movementCategoryPutDto == null ||movementCategoryPutDto ==  Guid.Empty) return true;

        return movementCategoryRecord.ParentCategory.GUID != movementCategoryPutDto;
        
    }
    
}
