using GainsLab.Contracts.Interface;

namespace GainsLab.Contracts.Dtos.PostDto.Outcome;

public sealed record MovementCategoryCreateCombineOutcome(
    MovementCategoryCreateOutcome? MovementCategory,
    DescriptorCreateOutcome? Descriptor,
    IMessagesContainer? Message = null);
