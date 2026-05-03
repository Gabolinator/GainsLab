using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.SummaryDto;
using GainsLab.Contracts.Interface;

namespace GainsLab.Contracts.Dtos.PostDto.Outcome;

public sealed record DescriptorCreateOutcome(CreateOutcome Outcome,DescriptorSummaryDTO? CreatedDescriptor,  IMessagesContainer? Message =null);

