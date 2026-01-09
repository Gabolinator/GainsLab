namespace GainsLab.Contracts.Dtos.PostDto.Request;

public sealed record DescriptorCreateRequest(
    DescriptorPostDTO? DescriptorPostDto,
    CreateRequest CreateRequest,
    string RequestedBy);


