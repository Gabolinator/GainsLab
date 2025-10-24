using System;
using GainsLab.Core.Models.Core.CreationInfo;
using GainsLab.Core.Models.Core.Entities.Descriptor;
using GainsLab.Core.Models.Core.Entities.Identifier;
using GainsLab.Core.Models.Core.Interfaces.Entity;
using GainsLab.Models.Core;


namespace GainsLab.Core.Models.Core.Entities.WorkoutEntity;

/// <summary>
/// Immutable data describing a movement, including workloads and supporting equipment.
/// </summary>
public sealed record MovementContent(
    string Name,
    MovementCategoryId Category,
    MuscleWorked MusclesWorked,
    EquipmentIdList EquipmentRequired
) : IEntityContent<MovementContent>
{
    public MovementContent Validate()
    {
        if (string.IsNullOrWhiteSpace(Name)) throw new ArgumentException("Movement name is required.", nameof(Name));
        return this;
    }
}

/// <summary>
/// Aggregate root representing a concrete movement definition.
/// </summary>
public sealed class MovementEntity
    : EntityBase<MovementId, MovementContent, AuditedInfo>,
        IDescribed<BaseDescriptorEntity>
{
    public BaseDescriptorEntity Descriptor { get; private set; }

    // public MovementEntity(MovementContent content, string createdBy, BaseDescriptorEntity? descriptor = null, int dbId = -1)
    //     : base(MovementId.New(), content.Validate(), AuditedInfo.New(createdBy), dbId)
    // {
    //     Descriptor = descriptor ?? new BaseDescriptorEntity();
    // }

    public override EntityType Type => EntityType.Movement;
    private MovementEntity() { Descriptor = new BaseDescriptorEntity(); }


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
    /// Returns a copy referencing the supplied descriptor entity.
    /// </summary>
    public MovementEntity WithDescriptor(BaseDescriptorEntity descriptor)
        => new() {
            Id = this.Id,
            Content = this.Content,
            CreationInfo = this.CreationInfo,
            Descriptor = descriptor
        };

    private MovementEntity WithContent(MovementContent newContent)
        => new() {
            Id = this.Id,
            Content = newContent,
            CreationInfo = this.CreationInfo,
            Descriptor = this.Descriptor
        };
}
