namespace GainsLab.Contracts.Dtos.UpdateDto.Request;

public sealed record MuscleUpdateRequest(
    MuscleUpdateDTO Muscle,
    Guid CorrelationId,
    UpdateRequest UpdateRequest,
    string RequestedBy);
