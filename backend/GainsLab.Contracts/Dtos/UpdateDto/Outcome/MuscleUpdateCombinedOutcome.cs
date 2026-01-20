using GainsLab.Contracts.Interface;

namespace GainsLab.Contracts.Dtos.UpdateDto.Outcome;

public sealed record MuscleUpdateCombinedOutcome(
    MuscleUpdateOutcome? Muscle,
    DescriptorUpdateOutcome? Descriptor,
    IMessagesContainer? Message = null);


