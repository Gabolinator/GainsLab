using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Interface;

namespace GainsLab.Contracts.Dtos.PostDto.Outcome;

public sealed record MuscleCreateOutcome(
    CreateOutcome Outcome,
    MuscleGetDTO? CreatedMuscle,
    IMessagesContainer? Message = null);
