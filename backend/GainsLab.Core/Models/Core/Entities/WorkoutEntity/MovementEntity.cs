using System;
using System.Collections.Generic;
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
    EquipmentIdList EquipmentRequired,
    (string? variantOfName, MovementId? variantOfGuid) variantOf  = default //need to resolve this in some way - after all movement created
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
    internal MovementEntity WithVariant(MovementId variantMovementId)
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

public sealed class MovementPersistenceModel
{
    public MovementPersistenceModel(
        int movementCategoryDbId,
        IReadOnlyDictionary<MuscleId, int>? muscleDbIds,
        IReadOnlyDictionary<EquipmentId, int>? equipmentDbIds)
    {
        MovementCategoryDbId = movementCategoryDbId;
        MuscleDbIds = muscleDbIds != null
            ? new Dictionary<MuscleId, int>(muscleDbIds)
            : new Dictionary<MuscleId, int>();
        EquipmentDbIds = equipmentDbIds != null
            ? new Dictionary<EquipmentId, int>(equipmentDbIds)
            : new Dictionary<EquipmentId, int>();
    }

    public int MovementCategoryDbId { get; init; }

    public IReadOnlyDictionary<MuscleId, int> MuscleDbIds { get; init; }

    public IReadOnlyDictionary<EquipmentId, int> EquipmentDbIds { get; init; }

    public static MovementPersistenceModel Empty { get; } =
        new(0, new Dictionary<MuscleId, int>(), new Dictionary<EquipmentId, int>());

    public bool TryGetMuscleDbId(MuscleId id, out int dbId) => MuscleDbIds.TryGetValue(id, out dbId);

    public bool TryGetEquipmentDbId(EquipmentId id, out int dbId) => EquipmentDbIds.TryGetValue(id, out dbId);

    public MovementPersistenceModel WithCategoryDbId(int categoryDbId) =>
        new(categoryDbId, MuscleDbIds, EquipmentDbIds);

    public MovementPersistenceModel WithMuscles(IReadOnlyDictionary<MuscleId, int> muscleDbIds) =>
        new(MovementCategoryDbId, muscleDbIds, EquipmentDbIds);

    public MovementPersistenceModel WithEquipment(IReadOnlyDictionary<EquipmentId, int> equipmentDbIds) =>
        new(MovementCategoryDbId, MuscleDbIds, equipmentDbIds);
}
