using GainsLab.Contracts.Dtos.ID;

namespace GainsLab.Contracts.Dtos.Delete.Outcome;

public sealed record MovementCategoryDeleteOutcome(EntityId Id, DeleteOutcome Outcome);
