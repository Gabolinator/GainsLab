using GainsLab.Contracts.Interface;

namespace GainsLab.Contracts.Dtos.PostDto.Outcome;

public sealed record MuscleCreateCombineOutcome(
    MuscleCreateOutcome? Muscle,
    DescriptorCreateOutcome? Descriptor,
    IMessagesContainer? Message = null);
