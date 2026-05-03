using GainsLab.Contracts.Dtos.SummaryDto;
using GainsLab.Domain;
using GainsLab.Domain.Entities.Identifier;

namespace GainsLab.Contracts.Dtos.GetDto;

public record MovementGetDTO(
    MovementId Id,
    string Name,
    DescriptorSummaryDTO? Descriptor,
    MovementCategorySummaryDTO? Category,
    IReadOnlyList<EquipmentSummaryDTO> Equipments,
    IReadOnlyList<SummaryDto.MuscleSummaryDTO> PrimaryMuscles,
    IReadOnlyList<SummaryDto.MuscleSummaryDTO> SecondaryMuscles,
    MovementSummaryDTO? VariantOfMovement,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    long UpdatedSeq,
    bool IsDeleted = false,
    DataAuthority Authority = DataAuthority.Bidirectional);