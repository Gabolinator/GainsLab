using GainsLab.Contracts.Interface;

namespace GainsLab.Contracts.Dtos.PostDto.Outcome;

public record EquipmentCreateCombineOutcome(
    EquipmentCreateOutcome? CreatedEquipmentOutcome,
    DescriptorCreateOutcome? CreatedDescriptorOutcome, IMessagesContainer? Message =null);