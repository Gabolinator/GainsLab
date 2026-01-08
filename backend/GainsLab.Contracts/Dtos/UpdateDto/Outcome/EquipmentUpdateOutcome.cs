using GainsLab.Contracts.Dtos.GetDto;

namespace GainsLab.Contracts.Dtos.UpdateDto.Outcome;

public sealed record EquipmentUpdateOutcome(
    UpdateOutcome Outcome,
    UpdateOutcome DescriptorOutcome,
    DescriptorUpdateOutcome? Descriptor,
    EquipmentGetDTO? UpdatedState,
    string? Message);


