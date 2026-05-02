using GainsLab.Domain.Entities.CreationInfo;
using GainsLab.Domain.Entities.Descriptor;
using GainsLab.Domain.Entities.Identifier;
using GainsLab.Domain.Entities.WorkoutEntity.EntityContent;
using GainsLab.Domain.Entities.WorkoutEntity.Persistence;
using GainsLab.Domain.Interfaces.Entity;

namespace GainsLab.Domain.Entities.WorkoutEntity;

/// <summary>
/// Aggregate root representing a concrete movement definition.
/// </summary>
public sealed class MovementEntity
    : EntityBase<MovementId, MovementContent, AuditedInfo>,
        IDescribed<BaseDescriptorEntity>
{
    public BaseDescriptorEntity Descriptor { get; private set; }
    public MovementPersistenceModel Persistence { get; private set; } = MovementPersistenceModel.Empty;

    public MovementEntity(
        MovementContent content, 
        MovementId id,
        AuditedInfo creation,
        BaseDescriptorEntity descriptor, 
        int dbId = -1,
        MovementPersistenceModel? persistence = null)
        : base(id, content, creation, dbId)
    {
        Descriptor = descriptor;
        Persistence = persistence ?? MovementPersistenceModel.Empty;
    }

    public override EntityType Type => EntityType.Movement;
    private MovementEntity()
    {
        Descriptor = new BaseDescriptorEntity();
        Persistence = MovementPersistenceModel.Empty;
    }


    /// <summary>
    /// Returns a copy with the movement name updated.
    /// </summary>
    public MovementEntity Rename(string newName)
        =>WithContent((Content with { Name = newName }).Validate());

    /// <summary>
    /// Returns a copy assigned to a different movement category.
    /// </summary>
    public MovementEntity Recat(MovementCategoryId newCategory)
        => WithContent(Content with { Category = newCategory });

    /// <summary>
    /// Returns a copy with the associated muscles updated.
    /// </summary>
    public MovementEntity WithMuscles(MuscleWorked muscles)
        => WithContent(Content with { MusclesWorked = muscles });

    /// <summary>
    /// Returns a copy with the required equipment updated.
    /// </summary>
    public MovementEntity WithEquipment(EquipmentIdList equipment)
        => WithContent(Content with { EquipmentRequired = equipment });
    
    /// <summary>
    /// Returns a copy referencing the supplied movement as its variant origin.
    /// </summary>
    public MovementEntity WithVariant(MovementId variantMovementId)
    {
        if (variantMovementId == default)
        {
            throw new ArgumentException("Variant movement id is required.", nameof(variantMovementId));
        }

        return WithContent(Content with { variantOf = (null,variantMovementId) });
    }

    /// <summary>
    /// Returns a copy referencing the supplied descriptor entity.
    /// </summary>
    public MovementEntity WithDescriptor(BaseDescriptorEntity descriptor)
        => new() {
            Id = this.Id,
            Content = this.Content,
            CreationInfo = this.CreationInfo,
            Descriptor = descriptor,
            DbId = this.DbId,
            Persistence = this.Persistence
        };

    private MovementEntity WithContent(MovementContent newContent)
        => new() {
            Id = this.Id,
            Content = newContent,
            CreationInfo = this.CreationInfo,
            Descriptor = this.Descriptor,
            DbId = this.DbId,
            Persistence = this.Persistence
        };

    internal MovementEntity WithPersistence(MovementPersistenceModel persistence)
        => new()
        {
            Id = this.Id,
            Content = this.Content,
            CreationInfo = this.CreationInfo,
            Descriptor = this.Descriptor,
            DbId = this.DbId,
            Persistence = persistence ?? MovementPersistenceModel.Empty
        };
}