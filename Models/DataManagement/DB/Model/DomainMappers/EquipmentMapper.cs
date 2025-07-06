using GainsLab.Models.Core;
using GainsLab.Models.Core.Descriptor;
using GainsLab.Models.DataManagement.DB.Model.DTOs;
using GainsLab.Models.WorkoutComponents.Equipment;

namespace GainsLab.Models.DataManagement.DB.Model.DomainMappers;

public static class EquipmentMapper
{
    public static EquipmentDTO ToDTO(Equipment domain)
    {
        //if id is negative - likely not yet inserted in db, dont keep id 
        //id id is positive - probably already inserted  - we keep it 

        return domain.Id <= 0 ? ToInsertDTO(domain) : ToUpdateDTO(domain);
    }
    
    public static EquipmentDTO ToInsertDTO(Equipment domain)
    {

        return new EquipmentDTO
        {
            //dont use id 
            UID = domain.UID!,
            DescriptorID = domain.Descriptor.Id,
            Descriptor = domain.Descriptor.ToDTO()
        };
    }
    public static EquipmentDTO ToUpdateDTO(Equipment domain)
    {
        return new EquipmentDTO
        {
            Id = domain.Id,  // Use existing Id
            UID = domain.UID!,
            DescriptorID = domain.Descriptor.Id,
            Descriptor = domain.Descriptor.ToDTO()
        };
    }

    public static Equipment ToDomain(EquipmentDTO dto)
    {
        return new Equipment(
            dto.Descriptor.ToDomain(),
            new Identifier(dto.Id, dto.UID, eWorkoutComponents.Equipment)
        );
    }
}