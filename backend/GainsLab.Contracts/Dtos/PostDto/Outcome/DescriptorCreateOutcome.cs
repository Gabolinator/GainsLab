using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Interface;

namespace GainsLab.Contracts.Dtos.PostDto.Outcome;

public sealed record DescriptorCreateOutcome(CreateOutcome Outcome,DescriptorGetDTO? CreatedDescriptor,  IMessagesContainer? Message =null);

