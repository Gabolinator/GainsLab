using GainsLab.Domain.Entities.Identifier;

namespace GainsLab.Domain.Entities.WorkoutEntity.Persistence;

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