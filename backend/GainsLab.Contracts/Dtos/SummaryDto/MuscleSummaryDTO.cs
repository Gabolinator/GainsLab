using GainsLab.Domain;
using GainsLab.Domain.Entities.Identifier;

namespace GainsLab.Contracts.Dtos.SummaryDto;

public record MuscleSummaryDTO(
    MuscleId Id,
    string Name,
    string? LatinName,
    eBodySection BodySection);