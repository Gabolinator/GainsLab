using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Interface;

namespace GainsLab.Contracts.Dtos.UpdateDto.Outcome;

public sealed record EquipmentUpdateOutcome(
    UpdateOutcome Outcome,
    UpdateOutcome DescriptorOutcome,
    DescriptorUpdateOutcome? Descriptor,
    EquipmentGetDTO? UpdatedState,
    IMessagesContainer? Message = null);


