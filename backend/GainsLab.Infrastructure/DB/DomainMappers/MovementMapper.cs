using System;
using System.Collections.Generic;
using System.Linq;
using GainsLab.Core.Models.Core;
using GainsLab.Core.Models.Core.CreationInfo;
using GainsLab.Core.Models.Core.Entities.Descriptor;
using GainsLab.Core.Models.Core.Entities.Identifier;
using GainsLab.Core.Models.Core.Entities.WorkoutEntity;
using GainsLab.Core.Models.Core.Interfaces.DB;
using GainsLab.Core.Models.Core.Utilities;
using GainsLab.Core.Models.Core.Utilities.Logging;
using GainsLab.Infrastructure.DB.DTOs;

namespace GainsLab.Infrastructure.DB.DomainMappers;

public static class MovementMapper
{
    private static bool _verbose = true;
    private static GainsLabLogger _logger = new GainsLabLogger("MovementMapper", GainsLabLogger.DecoratorColor.None);
    public static IDto? ToDTO(MovementEntity? domain)
    {
        if (domain == null) return null;

        var descriptorDto = (DescriptorDTO)domain.Descriptor?.ToDTO(); //BuildDescriptorDto(domain);
        var categoryDbId = domain.Persistence.MovementCategoryDbId;
        if (categoryDbId <= 0)
        {
            throw new InvalidOperationException(
                $"Movement '{domain.Content.Name}' does not have a resolved category database id. " +
                "Include MovementCategory when loading or supply persistence metadata before saving.");
        }

        var dto = new MovementDTO
        {
            Id = domain.DbId > 0 ? domain.DbId : 0,
            GUID = domain.Id,
            Name = domain.Content.Name,
            DescriptorID = descriptorDto.Id,
            Descriptor = descriptorDto,
            MovementCategoryId = categoryDbId,
            CreatedAtUtc = domain.CreationInfo.CreatedAtUtc,
            CreatedBy = domain.CreationInfo.CreatedBy,
            UpdatedAtUtc = domain.CreationInfo.UpdatedAtUtc ?? CoreUtilities.Clock.UtcNow,
            UpdatedBy = domain.CreationInfo.UpdatedBy,
            Version = domain.CreationInfo.Version,
            IsDeleted = domain.CreationInfo.IsDeleted,
            DeletedAt = domain.CreationInfo.DeletedAt,
            DeletedBy = domain.CreationInfo.DeletedBy,
            VariantOfMovementGuid = domain.Content.variantOf.variantOfGuid
        };

        dto.MuscleRelations = BuildMuscleRelations(domain, dto).ToList();
        dto.EquipmentRelations = BuildEquipmentRelations(domain, dto).ToList();

        return dto;
    }

    public static MovementEntity? ToDomain(MovementDTO? dto)
    {
        if (dto == null) return null;

        if (dto.Descriptor is null)
        {
            throw new InvalidOperationException(
                $"Movement '{dto.Name}' requires Descriptor data. Ensure the descriptor navigation is included when querying.");
        }

        if (dto.Category is null)
        {
            throw new InvalidOperationException(
                $"Movement '{dto.Name}' requires Category data. Include MovementCategory when querying.");
        }

        var descriptor = dto.Descriptor.ToDomain() as BaseDescriptorEntity
            ?? throw new InvalidOperationException("Unable to map movement descriptor.");

        var muscleWorked = CreateMuscleWorked(dto);
        var equipmentRequired = CreateEquipmentList(dto);
        
        var content = new MovementContent(
            dto.Name,
            new MovementCategoryId(dto.Category.GUID),
            muscleWorked,
            equipmentRequired,
            dto.VariantOfMovementGuid != null ? (null,new MovementId(dto.VariantOfMovementGuid.Value)) : default);

        var audited = new AuditedInfo(
            dto.CreatedAtUtc,
            dto.CreatedBy,
            dto.UpdatedAtUtc,
            dto.UpdatedBy,
            dto.Version,
            dto.IsDeleted,
            dto.DeletedAt,
            dto.DeletedBy);

        var persistence = new MovementPersistenceModel(
            dto.MovementCategoryId,
            BuildMuscleLookup(dto.MuscleRelations),
            BuildEquipmentLookup(dto.EquipmentRelations));

        return new MovementEntity(content, new MovementId(dto.GUID), audited, descriptor, dto.Id, persistence);
    }

    private static DescriptorDTO BuildDescriptorDto(MovementEntity domain)
    {
        if (domain.Descriptor == null)
        {
            throw new InvalidOperationException("Movement descriptor is required.");
        }

        var descriptorDto = domain.Descriptor.ToDTO() as DescriptorDTO
            ?? throw new InvalidOperationException("Unable to convert descriptor to DTO.");

        if (domain.Descriptor.DbId <= 0 && descriptorDto.Id <= 0)
        {
            throw new InvalidOperationException(
                "Descriptor must be persisted before the movement can be saved. Save the descriptor first to obtain a database id.");
        }

        descriptorDto.Id = domain.Descriptor.DbId > 0 ? domain.Descriptor.DbId : descriptorDto.Id;
        return descriptorDto;
    }

    private static IEnumerable<MovementMuscleRelationDTO> BuildMuscleRelations(MovementEntity domain, MovementDTO parent)
    {
        
        var muscles = domain.Content.MusclesWorked;

        
        if (muscles == null)
        {
            
            yield break;
        }
        
        if (_verbose) _logger.Log(nameof(MovementMapper), $"Build muscle relation - All muscles count: {muscles.GetAllMuscle().Count()}");


        foreach (var muscleId in muscles.PrimaryMuscles?.Ids ?? Enumerable.Empty<MuscleId>())
        {
            yield return CreateMuscleRelation(parent, domain, muscleId, MuscleRole.Primary);
        }

        foreach (var muscleId in muscles.SecondaryMuscles?.Ids ?? Enumerable.Empty<MuscleId>())
        {
            yield return CreateMuscleRelation(parent, domain, muscleId, MuscleRole.Secondary);
        }
    }

    private static MovementMuscleRelationDTO CreateMuscleRelation(
        MovementDTO parent,
        MovementEntity domain,
        MuscleId muscleId,
        MuscleRole role)
    {
        if (!domain.Persistence.TryGetMuscleDbId(muscleId, out var muscleDbId) || muscleDbId <= 0)
        {
            throw new InvalidOperationException(
                $"Movement '{parent.Name}' is missing the database id for muscle '{muscleId}'. Resolve the muscle id before saving.");
        }

        return new MovementMuscleRelationDTO
        {
            Movement = parent,
            MuscleId = muscleDbId,
            MuscleRole = role
        };
    }

    private static IEnumerable<MovementEquipmentRelationDTO> BuildEquipmentRelations(
        MovementEntity domain,
        MovementDTO parent)
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

            yield return new MovementEquipmentRelationDTO
            {
                Movement = parent,
                EquipmentId = dbId
            };
        }
    }

    private static MuscleWorked CreateMuscleWorked(MovementDTO dto)
    {
        var muscles = new MuscleWorked
        {
            PrimaryMuscles = new MuscleIdList(),
            SecondaryMuscles = new MuscleIdList()
        };

        muscles.PrimaryMuscles.AddUniques(dto.PrimaryMuscleGUIDs.Select(g => new MuscleId(g)));
        muscles.SecondaryMuscles.AddUniques(dto.SecondaryMuscleGUIDs.Select(g => new MuscleId(g)));
        return muscles;
    }

    private static EquipmentIdList CreateEquipmentList(MovementDTO dto)
    {
        var equipment = new EquipmentIdList();
        equipment.AddUniques(dto.EquipmentGUIDs.Select(g => new EquipmentId(g)));
        return equipment;
    }

    private static IReadOnlyDictionary<MuscleId, int> BuildMuscleLookup(
        IEnumerable<MovementMuscleRelationDTO> relations)
    {
        var lookup = new Dictionary<MuscleId, int>();

        foreach (var relation in relations ?? Array.Empty<MovementMuscleRelationDTO>())
        {
            if (relation.Muscle is null)
            {
                throw new InvalidOperationException(
                    "MovementMuscleRelationDTO must include the Muscle navigation to hydrate domain entities.");
            }

            lookup[new MuscleId(relation.Muscle.GUID)] = relation.MuscleId;
        }

        return lookup;
    }

    private static IReadOnlyDictionary<EquipmentId, int> BuildEquipmentLookup(
        IEnumerable<MovementEquipmentRelationDTO> relations)
    {
        var lookup = new Dictionary<EquipmentId, int>();

        foreach (var relation in relations ?? Array.Empty<MovementEquipmentRelationDTO>())
        {
            if (relation.Equipment is null)
            {
                throw new InvalidOperationException(
                    "MovementEquipmentRelationDTO must include the Equipment navigation to hydrate domain entities.");
            }

            lookup[new EquipmentId(relation.Equipment.GUID)] = relation.EquipmentId;
        }

        return lookup;
    }

    public static (IEnumerable<MovementMuscleRelationDTO> muscleRelations, IEnumerable<MovementEquipmentRelationDTO> equipmentRelations) CreateMovementRelationDTOs(IReadOnlyList<MovementDTO> movementDtos, IReadOnlyList<MovementEntity> movementDomainEntities)
    {
        if (movementDtos == null) throw new ArgumentNullException(nameof(movementDtos));
        _ = movementDomainEntities;

        var muscleRelations = new List<MovementMuscleRelationDTO>();
        var equipmentRelations = new List<MovementEquipmentRelationDTO>();

        foreach (var movementDto in movementDtos.Where(dto => dto != null))
        {
            foreach (var relation in movementDto.MuscleRelations ?? Enumerable.Empty<MovementMuscleRelationDTO>())
            {
                relation.Movement ??= movementDto;
                muscleRelations.Add(relation);
            }

            foreach (var relation in movementDto.EquipmentRelations ?? Enumerable.Empty<MovementEquipmentRelationDTO>())
            {
                relation.Movement ??= movementDto;
                equipmentRelations.Add(relation);
            }
        }

        return (muscleRelations, equipmentRelations);
    }
}
