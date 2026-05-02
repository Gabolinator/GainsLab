using GainsLab.Domain.Entities.CreationInfo;
using GainsLab.Domain.Entities.Descriptor;
using GainsLab.Domain.Entities.Identifier;
using GainsLab.Domain.Entities.WorkoutEntity.EntityContent;
using GainsLab.Domain.Interfaces.Entity;

namespace GainsLab.Domain.Entities.WorkoutEntity;

/// <summary>
/// Aggregate root for grouping movements under a shared category.
/// </summary>
public class MovementCategoryEntity : EntityBase<MovementCategoryId, MovementCategoryContent, AuditedInfo>,
    IDescribed<BaseDescriptorEntity>
{

    public MovementCategoryEntity(
        MovementCategoryContent content,
        MovementCategoryId id,
        AuditedInfo creation,
        BaseDescriptorEntity descriptor,
        int dbId = -1) : base(id, content, creation, dbId)
    {
        Descriptor = descriptor;
    }

    public BaseDescriptorEntity Descriptor { get; }
    
    /// <summary>
    /// Returns a copy with base movement categories replaced by the supplied set.
    /// </summary>
    public MovementCategoryEntity WithBaseCategories (params eMovementCategories[] baseCategories)
    {
        var content = new MovementCategoryContent(Content.Name, baseCategories);
        return new MovementCategoryEntity(content, Id, CreationInfo, Descriptor, DbId);
    }
    
    /// <summary>
    /// Returns a copy with base movement categories replaced by the supplied parent.
    /// </summary>
    public MovementCategoryEntity WithParentCategory (MovementCategoryId? parentId)
    {
        var content =  new MovementCategoryContent(Content.Name, Content.BaseCategories);
        content.ParentCategoryId = parentId;
        return new MovementCategoryEntity(content, Id, CreationInfo, Descriptor, DbId);
    }

    public override EntityType Type => EntityType.MovementCategory;
}
