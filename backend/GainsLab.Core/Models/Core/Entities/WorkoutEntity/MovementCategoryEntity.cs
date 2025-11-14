using System;
using GainsLab.Core.Models.Core.CreationInfo;
using GainsLab.Core.Models.Core.Entities.Descriptor;
using GainsLab.Core.Models.Core.Entities.Identifier;
using GainsLab.Core.Models.Core.Interfaces.Entity;
using GainsLab.Models.Core;

namespace GainsLab.Core.Models.Core.Entities.WorkoutEntity;

/// <summary>
/// Immutable details describing a movement category and its base categorizations.
/// </summary>
public record MovementCategoryContent(string Name , params eMovementCategories[] BaseCategories) : IEntityContent<MovementCategoryContent>
{
    public MovementCategoryId? ParentCategoryId { get; set; } = null;
    
    public MovementCategoryContent Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("MovementCategory name is required.", nameof(Name));

        if (BaseCategories is null || BaseCategories.Length == 0)
            throw new ArgumentException("At least one base category is required.", nameof(BaseCategories));

        if (BaseCategories.Contains(eMovementCategories.undefined))
            throw new ArgumentException("Base categories cannot include 'undefined'.", nameof(BaseCategories));

        return this;
    }
}
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
