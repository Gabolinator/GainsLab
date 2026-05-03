using GainsLab.Domain;
using GainsLab.Domain.Entities.Identifier;

namespace GainsLab.Contracts.Dtos.PutDto;

public record MovementPutDTO(
    MovementId Id,
    string Name,
    DescriptorId? DescriptorId,
    MovementCategoryId? CategoryId,
    IReadOnlyList<EquipmentId> EquipmentIds,
    IReadOnlyList<MuscleId> PrimaryMuscleIds,
    IReadOnlyList<MuscleId> SecondaryMuscleIds,
    MovementId? VariantOfMovementId,
    DataAuthority Authority = DataAuthority.Bidirectional);