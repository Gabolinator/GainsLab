using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Interface;

namespace GainsLab.Contracts.Dtos.UpdateDto.Outcome;

public sealed record DescriptorUpdateOutcome(
    UpdateOutcome Outcome,
    DescriptorGetDTO? UpdatedState,
    IMessagesContainer? Message =null);




