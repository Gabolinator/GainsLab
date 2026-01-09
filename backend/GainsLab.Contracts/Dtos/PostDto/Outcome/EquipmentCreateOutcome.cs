using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Interface;

namespace GainsLab.Contracts.Dtos.PostDto.Outcome;

public record EquipmentCreateOutcome(CreateOutcome Outcome,EquipmentGetDTO? CreatedEquipment,  IMessagesContainer? Message =null);


