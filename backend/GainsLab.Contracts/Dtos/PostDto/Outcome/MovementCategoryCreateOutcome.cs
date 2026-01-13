using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Interface;

namespace GainsLab.Contracts.Dtos.PostDto.Outcome;

public sealed record MovementCategoryCreateOutcome(
    CreateOutcome Outcome,
    MovementCategoryGetDTO? CreatedMovementCategory,
    IMessagesContainer? Message = null);
