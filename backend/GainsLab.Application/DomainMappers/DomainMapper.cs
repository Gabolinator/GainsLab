using GainsLab.Application.DTOs;
using GainsLab.Application.DTOs.Description;
using GainsLab.Application.DTOs.Equipment;
using GainsLab.Application.DTOs.Movement;
using GainsLab.Application.DTOs.MovementCategory;
using GainsLab.Application.DTOs.Muscle;
using GainsLab.Contracts.Interface;
using GainsLab.Domain;
using GainsLab.Domain.Entities.Descriptor;
using GainsLab.Domain.Entities.WorkoutEntity;
using GainsLab.Domain.Interfaces;
using GainsLab.Domain.Interfaces.Entity;

namespace GainsLab.Application.DomainMappers;

/// <summary>
/// Converts between domain entities and their persistence Record representations.
/// </summary>
public static class EntityDomainMapper
{

    /// <summary>
    /// Maps a domain entity to its corresponding Record when supported.
    /// </summary>
    /// <param name="domain">Domain entity to convert.</param>
    /// <returns>The Record representation or <c>null</c> when the type is unsupported.</returns>
    public static IRecord? ToRecord(this IEntity? domain, IClock clock)
    {
        if (domain == null) return null;


        return (domain.Type) switch
        {
            EntityType.Equipment => EquipmentMapper.ToRecord(domain as EquipmentEntity, clock),
            EntityType.Descriptor => DescriptorMapper.ToRecord(domain as BaseDescriptorEntity, clock),
            EntityType.Muscle => MuscleMapper.ToRecord(domain as MuscleEntity, clock),
            EntityType.MovementCategory => MovementCategoryMapper.ToRecord(domain as MovementCategoryEntity, clock),
            EntityType.Movement => MovementMapper.ToRecord(domain as MovementEntity, clock),
            //to eventually implement others
            _ => throw new ArgumentOutOfRangeException()
        };
        
    }
    
    /// <summary>
    /// Maps a Persistent Record to its corresponding domain entity when supported.
    /// </summary>
    /// <param name="Record">Record to convert.</param>
    /// <returns>The domain entity representation or <c>null</c> when the type is unsupported.</returns>
    public static IEntity? ToDomain(this IRecord? Record)
    {
        if (Record == null) return null;
        
        return (Record.Type) switch
        {
            EntityType.Equipment => EquipmentMapper.ToDomain(Record as EquipmentRecord),
            EntityType.Descriptor => DescriptorMapper.ToDomain(Record as DescriptorRecord),
            EntityType.Muscle => MuscleMapper.ToDomain(Record as MuscleRecord),
            EntityType.MovementCategory => MovementCategoryMapper.ToDomain(Record as MovementCategoryRecord),
            EntityType.Movement => MovementMapper.ToDomain(Record as MovementRecord),
            
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
//     public static IWorkoutComponent ToDomain(this IRecord Record)
//     {
//         return (Record) switch
//         {
//             EquipmentRecord equipmentRecord => EquipmentMapper.ToDomain(equipmentRecord),
//             //to eventually implement others
//             
//             _ => throw new ArgumentOutOfRangeException(nameof(Record))
//         };
//     }
//     
//     
// }
