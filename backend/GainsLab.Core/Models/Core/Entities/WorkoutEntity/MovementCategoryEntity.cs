using GainsLab.Core.Models.Core.CreationInfo;
using GainsLab.Core.Models.Core.Entities.Descriptor;
using GainsLab.Core.Models.Core.Entities.Identifier;
using GainsLab.Core.Models.Core.Interfaces.Entity;
using GainsLab.Models.Core;

namespace GainsLab.Core.Models.Core.Entities.WorkoutEntity;

public record MovementCategoryContent(string Name , params eMovementCategories[] BaseCategories) : IEntityContent<MovementCategoryContent>
{
    public MovementCategoryContent Validate()
    {
        if (string.IsNullOrWhiteSpace(Name)) throw new ArgumentException("MovementCategory name is required.", nameof(Name));
        return this;
    }
}


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
    
    public MovementCategoryEntity WithBaseCategories (params eMovementCategories[] baseCategories)
    {
        var content = new MovementCategoryContent(Content.Name, baseCategories);
        return new MovementCategoryEntity(content, Id, CreationInfo, Descriptor, DbId);
      
    }

    public override EntityType Type => EntityType.MovementCategory;
}