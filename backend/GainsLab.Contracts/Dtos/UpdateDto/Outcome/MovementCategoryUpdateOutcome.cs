using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Interface;

namespace GainsLab.Contracts.Dtos.UpdateDto.Outcome;

public sealed record MovementCategoryUpdateOutcome(
    UpdateOutcome Outcome,
    UpdateOutcome DescriptorOutcome,
    DescriptorUpdateOutcome? Descriptor,
    MovementCategoryGetDTO? UpdatedState,
    IMessagesContainer? Message = null);
