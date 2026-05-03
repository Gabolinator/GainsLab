using GainsLab.Domain.Entities.Identifier;

namespace GainsLab.Contracts.Dtos.SummaryDto;

public record EquipmentSummaryDTO(
    EquipmentId Id,
    string Name);