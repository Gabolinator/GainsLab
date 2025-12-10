using System.ComponentModel.DataAnnotations;
using GainsLab.Domain;

namespace GainsLab.Application.DTOs;

public class MovementCategoryRecord : BaseRecord
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public Guid GUID { get; set; }

    public int DescriptorID { get; set; }

    public DescriptorRecord? Descriptor { get; set; }

    public int? ParentCategoryDbId { get; set; }

    public MovementCategoryRecord? ParentCategory { get; set; }

    //base categories for this category - we will have one category parent created for each of the base enum elements eMovementCategories - they will be used here
    public ICollection<MovementCategoryRelationRecord> BaseCategoryLinks { get; set; } = new List<MovementCategoryRelationRecord>();

    //to which other movement category is this category a parent 
    public ICollection<MovementCategoryRelationRecord> ChildCategoryLinks { get; set; } = new List<MovementCategoryRelationRecord>();

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

public class MovementCategoryRelationRecord
{
    public int ParentCategoryId { get; set; }

    public MovementCategoryRecord ParentCategory { get; set; } = null!;

    public int ChildCategoryId { get; set; }

    public MovementCategoryRecord ChildCategory { get; set; } = null!;
    
}
