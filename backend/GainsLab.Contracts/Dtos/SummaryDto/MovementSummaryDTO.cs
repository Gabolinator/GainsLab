using GainsLab.Domain.Entities.Identifier;

namespace GainsLab.Contracts.Dtos.SummaryDto;

public record MovementSummaryDTO(
    MovementId Id,
    string Name,
    MovementCategoryId? CategoryId,
    string? CategoryName,
    MovementId? VariantOfMovementId);