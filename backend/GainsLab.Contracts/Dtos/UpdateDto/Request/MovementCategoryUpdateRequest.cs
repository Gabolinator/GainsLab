namespace GainsLab.Contracts.Dtos.UpdateDto.Request;

public sealed record MovementCategoryUpdateRequest(
    MovementCategoryUpdateDTO? MovementCategory,
    Guid CorrelationId,
    UpdateRequest UpdateRequest,
    string RequestedBy);
