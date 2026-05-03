using GainsLab.Domain.Entities.Identifier;

namespace GainsLab.Contracts.Dtos.SummaryDto;

public record MovementCategorySummaryDTO(
    MovementCategoryId Id,
    string Name);