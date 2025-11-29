using GainsLab.Core.Models.Core;
using GainsLab.Core.Models.Core.Entities.Descriptor;
using GainsLab.Core.Models.Core.Entities.WorkoutEntity;
using GainsLab.Core.Models.Core.Interfaces.DB;
using GainsLab.Core.Models.Core.Interfaces.Entity;
using GainsLab.Infrastructure.DB.DTOs;
using GainsLab.Models.DataManagement.DB.Model.DomainMappers;

namespace GainsLab.Infrastructure.DB.DomainMappers;

/// <summary>
/// Converts between domain entities and their persistence DTO representations.
/// </summary>
public static class EntityDomainMapper
{

    /// <summary>
    /// Maps a domain entity to its corresponding DTO when supported.
    /// </summary>
    /// <param name="domain">Domain entity to convert.</param>
    /// <returns>The DTO representation or <c>null</c> when the type is unsupported.</returns>
    public static IDto? ToDTO(this IEntity? domain)
    {
        if (domain == null) return null;


        return (domain.Type) switch
        {
            EntityType.Equipment => EquipmentMapper.ToDTO(domain as EquipmentEntity),
            EntityType.Descriptor => DescriptorMapper.ToDTO(domain as BaseDescriptorEntity),
            EntityType.Muscle => MuscleMapper.ToDTO(domain as MuscleEntity),
            EntityType.MovementCategory => MovementCategoryMapper.ToDTO(domain as MovementCategoryEntity),
            EntityType.Movement => MovementMapper.ToDTO(domain as MovementEntity),
            //to eventually implement others
            _ => throw new ArgumentOutOfRangeException()
        };
        
    }
    
    /// <summary>
    /// Maps a DTO to its corresponding domain entity when supported.
    /// </summary>
    /// <param name="dto">DTO to convert.</param>
    /// <returns>The domain entity representation or <c>null</c> when the type is unsupported.</returns>
    public static IEntity? ToDomain(this IDto? dto)
    {
        if (dto == null) return null;
        
        return (dto.Type) switch
        {
            EntityType.Equipment => EquipmentMapper.ToDomain(dto as EquipmentDTO),
            EntityType.Descriptor => DescriptorMapper.ToDomain(dto as DescriptorDTO),
            EntityType.Muscle => MuscleMapper.ToDomain(dto as MuscleDTO),
            EntityType.MovementCategory => MovementCategoryMapper.ToDomain(dto as MovementCategoryDTO),
            EntityType.Movement => MovementMapper.ToDomain(dto as MovementDTO),
            
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
