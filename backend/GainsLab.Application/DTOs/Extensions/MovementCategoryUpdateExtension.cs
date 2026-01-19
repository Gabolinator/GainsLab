using System;
using System.Collections.Generic;
using System.Linq;
using GainsLab.Application.DTOs.MovementCategory;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.PutDto;
using GainsLab.Contracts.Dtos.UpdateDto;
using GainsLab.Domain;
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
               || ParentChanged(record, dto.ParentCategory?.Id)
               || ListChanged(record.BaseCategoryGUIDs, bases)
               || ListChanged(record.ChildGUIDs , childs);
    }
    
    
    public static bool TryUpdate(this MovementCategoryRecord record, MovementCategoryUpdateDTO dto, IClock clock,
        MovementCategoryRecord? parentCategory = null,
        ICollection<MovementCategoryRelationRecord>? baseCategoryRelations = null,
        ILogger? logger = null)
    {
        logger?.Log(
            nameof(MovementCategoryUpdateExtension),
            $"TryUpdate - Category:{record.GUID} Name:{record.Name} IncomingName:{dto.Name ?? "null"} Authority:{dto.Authority?.ToString() ?? "null"} BaseCount:{baseCategoryRelations?.Count.ToString() ?? "null"}");

        bool anyUpdate = false;

        if (AsNameChanged(record.Name, dto.Name))
        {
            var oldName = record.Name;
            record.Name = dto.Name!;
            anyUpdate = true;
            logger?.Log(nameof(MovementCategoryUpdateExtension),$"Name changed from '{oldName}' to '{record.Name}'");
        }
        
        
        
        if (AsAuthorityChanged(record.Authority, dto.Authority))
        {
            var oldAuthority = record.Authority;
            record.Authority = dto.Authority!.Value;
            anyUpdate = true;
            logger?.Log(nameof(MovementCategoryUpdateExtension),$"Authority changed from {oldAuthority} to {record.Authority}");

        }

        
        if(AsParentChanged(record.ParentCategory, dto.ParentCategory))
        {
            var oldParent = record.ParentCategory?.GUID;
            record.ParentCategory = parentCategory;
            
            record.ParentCategoryDbId = record.ParentCategory == null? null: parentCategory!.Id;
            anyUpdate = true;
            logger?.Log(nameof(MovementCategoryUpdateExtension),
                $"Parent changed from {(oldParent?.ToString() ?? "none")} to {(record.ParentCategory?.GUID.ToString() ?? "none")}");

            
        }

        // if (baseCategoryRelations != null && HaveBaseCategoriesChanged(record.BaseCategoryLinks,baseCategoryRelations ))
        // {
        //     record.BaseCategoryLinks.Clear();
        //     if (baseCategoryRelations != null)
        //     {
        //         foreach (var relation in baseCategoryRelations)
        //         {
        //             record.BaseCategoryLinks.Add(relation);
        //         }
        //     }
        //     anyUpdate = true;
        //     var existingBaseIds = record.BaseCategoryLinks.Select(link => link.ParentCategory?.GUID).Where(g => g.HasValue)
        //         .Select(g => g!.Value).ToList();
        //     logger?.Log(nameof(MovementCategoryUpdateExtension),
        //         $"Base categories updated. New set: {(existingBaseIds.Any() ? string.Join(',', existingBaseIds) : "none")}");
        // }
        
        if(!anyUpdate)
        {
            logger?.Log(nameof(MovementCategoryUpdateExtension),
                $"TryUpdate - No changes detected for Category:{record.GUID}");
            return false;
        }
        
        record.UpdatedAtUtc = clock.UtcNow;
        record.UpdatedBy = dto.UpdatedBy;
        logger?.Log(nameof(MovementCategoryUpdateExtension),
            $"TryUpdate - Updated timestamps for Category:{record.GUID} at {record.UpdatedAtUtc:o} by {record.UpdatedBy ?? "unknown"}");
        
        return true;
        
        
        bool AsNameChanged(string oldName, string? newName)
        {
            if(string.IsNullOrEmpty(newName)) return false;
            return !string.Equals(oldName, newName, StringComparison.InvariantCultureIgnoreCase);
            
        }
        
        bool AsAuthorityChanged(DataAuthority oldAuthority, DataAuthority? newAuthority)
        {
            if(newAuthority == null) return false;
            return oldAuthority !=newAuthority.Value;
        }
        
        bool AsParentChanged(MovementCategoryRecord? oldParentCategory, MovementCategoryRefDTO? newParentCategory)
        {
            
            if(oldParentCategory == null && newParentCategory == null) return false;
            if(oldParentCategory != null && newParentCategory == null || oldParentCategory == null && newParentCategory != null) return true;
            
            return oldParentCategory!.GUID != newParentCategory!.Id;
            
        }
        
        bool HaveBaseCategoriesChanged(IEnumerable<MovementCategoryRelationRecord>? oldCategory,IEnumerable<MovementCategoryRelationRecord>? newCategory )
        {
            var oldCount = oldCategory?.Count() ?? 0;
            var newCount = newCategory?.Count() ?? 0;
            if(oldCount == 0 && newCount ==0) return false;
            
            if(oldCount != newCount) return true;
            
            var existingBaseGuids =
                record.BaseCategoryLinks?
                          .Select(link => link.ParentCategory?.GUID)
                          .Where(guid => guid.HasValue && guid.Value != Guid.Empty)
                          .Select(guid => guid!.Value)
                          .OrderBy(guid => guid)
                          .ToArray()
                      ?? Array.Empty<Guid>();
            
            var incomingBaseGuids =
                baseCategoryRelations?
                        .Select(link => link.ParentCategory?.GUID)
                        .Where(guid => guid.HasValue && guid.Value != Guid.Empty)
                        .Select(guid => guid!.Value)
                        .OrderBy(guid => guid)
                        .ToArray() ?? Array.Empty<Guid>();
     
            if(existingBaseGuids.Length !=  incomingBaseGuids.Length) return true;
            return !existingBaseGuids.SequenceEqual(incomingBaseGuids);
            
        }

        
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
