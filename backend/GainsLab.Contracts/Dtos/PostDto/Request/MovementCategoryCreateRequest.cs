namespace GainsLab.Contracts.Dtos.PostDto.Request;

public sealed record MovementCategoryCreateRequest(
    MovementCategoryPostDTO? MovementCategory,
    CreateRequest CreateRequest,
    string RequestedBy);

public sealed record MovementCategoryCombineCreateRequest(
    MovementCategoryCreateRequest MovementCategory,
    DescriptorCreateRequest? Descriptor);
