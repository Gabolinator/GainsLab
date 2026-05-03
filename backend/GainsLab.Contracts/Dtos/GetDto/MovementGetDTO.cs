using GainsLab.Domain.Entities.Identifier;

namespace GainsLab.Contracts.Dtos.GetDto;

public record MovementGetDTO
{
    public MovementId Id { get; init; }
}