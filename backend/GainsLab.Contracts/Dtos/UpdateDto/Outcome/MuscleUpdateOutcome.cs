using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Interface;

namespace GainsLab.Contracts.Dtos.UpdateDto.Outcome;

public sealed record MuscleUpdateOutcome(
    UpdateOutcome Outcome,
    UpdateOutcome DescriptorOutcome,
    DescriptorUpdateOutcome? Descriptor,
    MuscleGetDTO? UpdatedState,
    IMessagesContainer? Message = null);
