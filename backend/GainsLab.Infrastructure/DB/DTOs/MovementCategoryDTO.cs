using System.ComponentModel.DataAnnotations;
using GainsLab.Core.Models.Core;

namespace GainsLab.Infrastructure.DB.DTOs;

public class MovementCategoryDTO : BaseDto
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public Guid GUID { get; set; }

    public int DescriptorID { get; set; }

    public DescriptorDTO? Descriptor { get; set; }

    public int? ParentCategoryDbId { get; set; }

    public MovementCategoryDTO? ParentCategory { get; set; }

    //base categories for this category - we will have one category parent created for each of the base enum elements eMovementCategories - they will be used here
    public ICollection<MovementCategoryRelationDTO> BaseCategoryLinks { get; set; } = new List<MovementCategoryRelationDTO>();

    //to which other movement category is this category a parent 
    public ICollection<MovementCategoryRelationDTO> ChildCategoryLinks { get; set; } = new List<MovementCategoryRelationDTO>();

    public override string? GetContent() => Name;

    public override int Iid => Id;

    public override Guid Iguid => GUID;

    public override EntityType Type => EntityType.MovementCategory;

    /// <summary>
    /// Convenience accessor used by domain mappers.
    /// </summary>
    public IEnumerable<Guid> ChildGUIDs => ChildCategoryLinks.Select(link => link.ChildCategory.GUID).Distinct();

    public IEnumerable<Guid> BaseCategoryGUIDs => BaseCategoryLinks.Select(link => link.ParentCategory.GUID).Distinct();

    // public bool TryGetBaseCategoryFromThis(out eMovementCategories category)
    // {
    //     category = eMovementCategories.undefined;
    //     if (string.IsNullOrWhiteSpace(Name)) return false;
    //     
    //     return Enum.TryParse(Name, true, out category);
    // }
    //
    //
 
}

public class MovementCategoryRelationDTO
{
    public int ParentCategoryId { get; set; }

    public MovementCategoryDTO ParentCategory { get; set; } = null!;

    public int ChildCategoryId { get; set; }

    public MovementCategoryDTO ChildCategory { get; set; } = null!;
    
}
