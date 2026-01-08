namespace GainsLab.Contracts.Dtos.UpdateDto;

public sealed record DescriptorUpdateRequest( 
    DescriptorUpdateDTO Descriptor,
    Guid CorrelationId,
    UpdateRequest UpdateRequest,
    string RequestedBy);

    
