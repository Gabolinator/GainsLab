using System;
using GainsLab.Models.Core;
using GainsLab.Models.Core.Interfaces;
using GainsLab.Models.DataManagement.DB.Model.DTOs;
using GainsLab.Models.WorkoutComponents.Equipment;

namespace GainsLab.Models.DataManagement.DB.Model.DomainMappers;

public static class WorkoutComponentDomainMapper
{

    public static IDto ToDTO(this IWorkoutComponent domain)
    {
        return (domain.ComponentType) switch
        {
            eWorkoutComponents.Equipment => EquipmentMapper.ToDTO(domain as Equipment),
            
            //to eventually implement others
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    
    
    
    
    public static IWorkoutComponent ToDomain(this IDto dto)
    {
        return (dto) switch
        {
            EquipmentDTO equipmentDto => EquipmentMapper.ToDomain(equipmentDto),
            //to eventually implement others
            
            _ => throw new ArgumentOutOfRangeException(nameof(dto))
        };
    }
    
    
}