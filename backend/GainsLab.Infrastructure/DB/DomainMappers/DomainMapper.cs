using System;
using GainsLab.Core.Models.Core.Entities.Descriptor;
using GainsLab.Core.Models.Core.Entities.WorkoutEntity;
using GainsLab.Core.Models.Core.Interfaces.Entity;
using GainsLab.Infrastructure.DB.DomainMappers;
using GainsLab.Models.Core;
using GainsLab.Models.DataManagement.DB.Model.DTOs;


namespace GainsLab.Models.DataManagement.DB.Model.DomainMappers;

public static class EntityDomainMapper
{

    public static IDto? ToDTO(this IEntity? domain)
    {
        if (domain == null) return null;



        return (domain.Type) switch
        {
            EntityType.Equipment => EquipmentMapper.ToDTO(domain as EquipmentEntity),
            EntityType.Descriptor => DescriptorMapper.ToDTO(domain as BaseDescriptorEntity),
            //to eventually implement others
            _ => throw new ArgumentOutOfRangeException()
        };
        
    }
    
    
    
}
//
//     
//     
//     
//     
//     public static IWorkoutComponent ToDomain(this IDto dto)
//     {
//         return (dto) switch
//         {
//             EquipmentDTO equipmentDto => EquipmentMapper.ToDomain(equipmentDto),
//             //to eventually implement others
//             
//             _ => throw new ArgumentOutOfRangeException(nameof(dto))
//         };
//     }
//     
//     
// }