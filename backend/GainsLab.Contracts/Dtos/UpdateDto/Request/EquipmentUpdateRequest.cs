namespace GainsLab.Contracts.Dtos.UpdateDto.Request;

public sealed record EquipmentUpdateRequest(
    EquipmentUpdateDTO Equipment,
    Guid CorrelationId,
    UpdateRequest UpdateRequest,
    string RequestedBy);