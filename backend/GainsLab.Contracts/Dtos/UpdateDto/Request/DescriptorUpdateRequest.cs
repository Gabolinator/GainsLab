namespace GainsLab.Contracts.Dtos.UpdateDto.Request;

public sealed record DescriptorUpdateRequest( 
    DescriptorUpdateDTO Descriptor,
    Guid CorrelationId,
    UpdateRequest UpdateRequest,
    string RequestedBy);

    
