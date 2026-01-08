namespace GainsLab.Contracts.Dtos.UpdateDto;

public sealed record EquipmentUpdateRequest(
    EquipmentUpdateDTO Equipment,
    Guid CorrelationId,
    UpdateRequest UpdateRequest,
    string RequestedBy);