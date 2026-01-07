using GainsLab.Application.DTOs;
using GainsLab.Application.DTOs.Description;
using GainsLab.Application.DTOs.Movement;
using GainsLab.Domain;
using GainsLab.Domain.Entities.CreationInfo;
using GainsLab.Domain.Entities.Descriptor;
using GainsLab.Domain.Entities.Identifier;
using GainsLab.Domain.Entities.WorkoutEntity;
using GainsLab.Domain.Interfaces;

namespace GainsLab.Application.DomainMappers;

public static class MovementMapper
{
    private static bool _verbose = true;
    public static MovementRecord? ToRecord(MovementEntity? domain, IClock clock)
    {
        if (domain == null) return null;

        var descriptorRecord = domain.Descriptor?.ToRecord(clock); //BuildDescriptorRecord(domain);
        var categoryDbId = domain.Persistence.MovementCategoryDbId;
        if (categoryDbId <= 0)
        {
            throw new InvalidOperationException(
                $"Movement '{domain.Content.Name}' does not have a resolved category database id. " +
                "Include MovementCategory when loading or supply persistence metadata before saving.");
        }

        var Record = new MovementRecord
        {
            Id = domain.DbId > 0 ? domain.DbId : 0,
            GUID = domain.Id,
            Name = domain.Content.Name,
            DescriptorID = descriptorRecord.Id,
            Descriptor = descriptorRecord,
            MovementCategoryId = categoryDbId,
            CreatedAtUtc = domain.CreationInfo.CreatedAtUtc,
            CreatedBy = domain.CreationInfo.CreatedBy,
            UpdatedAtUtc = domain.CreationInfo.UpdatedAtUtc ?? clock.UtcNow,
            UpdatedBy = domain.CreationInfo.UpdatedBy,
            Version = domain.CreationInfo.Version,
            IsDeleted = domain.CreationInfo.IsDeleted,
            DeletedAt = domain.CreationInfo.DeletedAt,
            DeletedBy = domain.CreationInfo.DeletedBy,
            VariantOfMovementGuid = domain.Content.variantOf.variantOfGuid
        };

        Record.MuscleRelations = BuildMuscleRelations(domain, Record).ToList();
        Record.EquipmentRelations = BuildEquipmentRelations(domain, Record).ToList();

        return Record;
    }

    public static MovementEntity? ToDomain(MovementRecord? Record)
    {
        if (Record == null) return null;

        if (Record.Descriptor is null)
        {
            throw new InvalidOperationException(
                $"Movement '{Record.Name}' requires Descriptor data. Ensure the descriptor navigation is included when querying.");
        }

        if (Record.Category is null)
        {
            throw new InvalidOperationException(
                $"Movement '{Record.Name}' requires Category data. Include MovementCategory when querying.");
        }

        var descriptor = Record.Descriptor.ToDomain() as BaseDescriptorEntity
            ?? throw new InvalidOperationException("Unable to map movement descriptor.");

        var muscleWorked = CreateMuscleWorked(Record);
        var equipmentRequired = CreateEquipmentList(Record);
        
        var content = new MovementContent(
            Record.Name,
            new MovementCategoryId(Record.Category.GUID),
            muscleWorked,
            equipmentRequired,
            Record.VariantOfMovementGuid != null ? (null,new MovementId(Record.VariantOfMovementGuid.Value)) : default);

        var audited = new AuditedInfo(
            Record.CreatedAtUtc,
            Record.CreatedBy,
            Record.UpdatedAtUtc,
            Record.UpdatedBy,
            Record.Version,
            Record.IsDeleted,
            Record.DeletedAt,
            Record.DeletedBy);

        var persistence = new MovementPersistenceModel(
            Record.MovementCategoryId,
            BuildMuscleLookup(Record.MuscleRelations),
            BuildEquipmentLookup(Record.EquipmentRelations));

        return new MovementEntity(content, new MovementId(Record.GUID), audited, descriptor, Record.Id, persistence);
    }

    private static DescriptorRecord BuildDescriptorRecord(MovementEntity domain, IClock clock)
    {
        if (domain.Descriptor == null)
        {
            throw new InvalidOperationException("Movement descriptor is required.");
        }

        var descriptorRecord = domain.Descriptor.ToRecord(clock) as DescriptorRecord
            ?? throw new InvalidOperationException("Unable to convert descriptor to Record.");

        if (domain.Descriptor.DbId <= 0 && descriptorRecord.Id <= 0)
        {
            throw new InvalidOperationException(
                "Descriptor must be persisted before the movement can be saved. Save the descriptor first to obtain a database id.");
        }

        descriptorRecord.Id = domain.Descriptor.DbId > 0 ? domain.Descriptor.DbId : descriptorRecord.Id;
        return descriptorRecord;
    }

    private static IEnumerable<MovementMuscleRelationRecord> BuildMuscleRelations(MovementEntity domain, MovementRecord parent)
    {
        
        var muscles = domain.Content.MusclesWorked;

        
        if (muscles == null)
        {
            
            yield break;
        }
        
     

        foreach (var muscleId in muscles.PrimaryMuscles?.Ids ?? Enumerable.Empty<MuscleId>())
        {
            yield return CreateMuscleRelation(parent, domain, muscleId, MuscleRole.Primary);
        }

        foreach (var muscleId in muscles.SecondaryMuscles?.Ids ?? Enumerable.Empty<MuscleId>())
        {
            yield return CreateMuscleRelation(parent, domain, muscleId, MuscleRole.Secondary);
        }
    }

    private static MovementMuscleRelationRecord CreateMuscleRelation(
        MovementRecord parent,
        MovementEntity domain,
        MuscleId muscleId,
        MuscleRole role)
    {
        if (!domain.Persistence.TryGetMuscleDbId(muscleId, out var muscleDbId) || muscleDbId <= 0)
        {
            throw new InvalidOperationException(
                $"Movement '{parent.Name}' is missing the database id for muscle '{muscleId}'. Resolve the muscle id before saving.");
        }

        return new MovementMuscleRelationRecord
        {
            Movement = parent,
            MuscleId = muscleDbId,
            MuscleRole = role
        };
    }

    private static IEnumerable<MovementEquipmentRelationRecord> BuildEquipmentRelations(
        MovementEntity domain,
        MovementRecord parent)
    {
        var equipment = domain.Content.EquipmentRequired;
        if (equipment?.Ids == null || equipment.Ids.Count == 0)
        {
            yield break;
        }

        foreach (var equipmentId in equipment.Ids)
        {
            if (!domain.Persistence.TryGetEquipmentDbId(equipmentId, out var dbId) || dbId <= 0)
            {
                throw new InvalidOperationException(
                    $"Movement '{parent.Name}' is missing the database id for equipment '{equipmentId}'. Resolve the equipment id before saving.");
            }

            yield return new MovementEquipmentRelationRecord
            {
                Movement = parent,
                EquipmentId = dbId
            };
        }
    }

    private static MuscleWorked CreateMuscleWorked(MovementRecord Record)
    {
        var muscles = new MuscleWorked
        {
            PrimaryMuscles = new MuscleIdList(),
            SecondaryMuscles = new MuscleIdList()
        };

        muscles.PrimaryMuscles.AddUniques(Record.PrimaryMuscleGUIDs.Select(g => new MuscleId(g)));
        muscles.SecondaryMuscles.AddUniques(Record.SecondaryMuscleGUIDs.Select(g => new MuscleId(g)));
        return muscles;
    }

    private static EquipmentIdList CreateEquipmentList(MovementRecord Record)
    {
        var equipment = new EquipmentIdList();
        equipment.AddUniques(Record.EquipmentGUIDs.Select(g => new EquipmentId(g)));
        return equipment;
    }

    private static IReadOnlyDictionary<MuscleId, int> BuildMuscleLookup(
        IEnumerable<MovementMuscleRelationRecord> relations)
    {
        var lookup = new Dictionary<MuscleId, int>();

        foreach (var relation in relations ?? Array.Empty<MovementMuscleRelationRecord>())
        {
            if (relation.Muscle is null)
            {
                throw new InvalidOperationException(
                    "MovementMuscleRelationRecord must include the Muscle navigation to hydrate domain entities.");
            }

            lookup[new MuscleId(relation.Muscle.GUID)] = relation.MuscleId;
        }

        return lookup;
    }

    private static IReadOnlyDictionary<EquipmentId, int> BuildEquipmentLookup(
        IEnumerable<MovementEquipmentRelationRecord> relations)
    {
        var lookup = new Dictionary<EquipmentId, int>();

        foreach (var relation in relations ?? Array.Empty<MovementEquipmentRelationRecord>())
        {
            if (relation.Equipment is null)
            {
                throw new InvalidOperationException(
                    "MovementEquipmentRelationRecord must include the Equipment navigation to hydrate domain entities.");
            }

            lookup[new EquipmentId(relation.Equipment.GUID)] = relation.EquipmentId;
        }

        return lookup;
    }

    public static (IEnumerable<MovementMuscleRelationRecord> muscleRelations, IEnumerable<MovementEquipmentRelationRecord> equipmentRelations) CreateMovementRelationRecords(IReadOnlyList<MovementRecord> movementRecords, IReadOnlyList<MovementEntity> movementDomainEntities)
    {
        if (movementRecords == null) throw new ArgumentNullException(nameof(movementRecords));
        _ = movementDomainEntities;

        var muscleRelations = new List<MovementMuscleRelationRecord>();
        var equipmentRelations = new List<MovementEquipmentRelationRecord>();

        foreach (var movementRecord in movementRecords.Where(Record => Record != null))
        {
            foreach (var relation in movementRecord.MuscleRelations ?? Enumerable.Empty<MovementMuscleRelationRecord>())
            {
                relation.Movement ??= movementRecord;
                muscleRelations.Add(relation);
            }

            foreach (var relation in movementRecord.EquipmentRelations ?? Enumerable.Empty<MovementEquipmentRelationRecord>())
            {
                relation.Movement ??= movementRecord;
                equipmentRelations.Add(relation);
            }
        }

        return (muscleRelations, equipmentRelations);
    }
}
