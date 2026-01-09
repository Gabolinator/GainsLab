namespace GainsLab.Contracts.Dtos.PostDto.Request;

public sealed record EquipmentCreateRequest(
    EquipmentPostDTO? Equipment,
    CreateRequest CreateRequest,
    string RequestedBy);


public sealed record EquipmentCombineCreateRequest(
    EquipmentCreateRequest Equipment,
    DescriptorCreateRequest? Descriptor);



