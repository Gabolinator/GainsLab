namespace GainsLab.Contracts.Dtos.PostDto.Request;

public sealed record MuscleCreateRequest(
    MusclePostDTO? Muscle,
    CreateRequest CreateRequest,
    string RequestedBy);

public sealed record MuscleCombineCreateRequest(
    MuscleCreateRequest Muscle,
    DescriptorCreateRequest? Descriptor);
