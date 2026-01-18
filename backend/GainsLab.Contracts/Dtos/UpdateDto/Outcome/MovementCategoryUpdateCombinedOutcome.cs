using GainsLab.Contracts.Interface;

namespace GainsLab.Contracts.Dtos.UpdateDto.Outcome;

public sealed record MovementCategoryUpdateCombinedOutcome(
    MovementCategoryUpdateOutcome? MovementCategory,
    DescriptorUpdateOutcome? Descriptor,
    IMessagesContainer? Message = null)
{
 
}
