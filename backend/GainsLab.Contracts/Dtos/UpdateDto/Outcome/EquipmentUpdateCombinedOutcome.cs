using GainsLab.Contracts.Interface;

namespace GainsLab.Contracts.Dtos.UpdateDto.Outcome;

public sealed record EquipmentUpdateCombinedOutcome(
    EquipmentUpdateOutcome? Equipment,
    DescriptorUpdateOutcome? DescriptorUpdateOutcome,
    IMessagesContainer? Message);



    
