using System;
using System.Collections.Generic;
using System.Linq;
using GainsLab.Models.Core;
using GainsLab.Models.WorkoutComponents.Equipment;
using GainsLab.Models.WorkoutComponents.MovementCategory;
using GainsLab.Models.WorkoutComponents.Muscle;


namespace GainsLab.Models.WorkoutComponents.Movement;

public class MovementContent : ComponentContent
{
    public MovementContent()
    {
        
    }

    public bool HasMovementVariant => VariantOfReference != null 
                                      && VariantOfReference.IsComponentResolved;
    public bool RequireEquipment => EquipmentListReference != null 
                                    && EquipmentListReference.IsComponentResolved 
                                    && EquipmentListReference.Component!.Items.Count >0;

    public bool HasMuscleGroup => MusclesWorkedReference != null
                                  && MusclesWorkedReference.IsComponentResolved
                                  && MusclesWorkedReference.Component!.GetAllMuscles().Items.Count > 0;
    
    #region Getter

    public ComponentReference<Movement>? VariantOfReference 
        =>  GetReferencesOfType<Movement>().FirstOrDefault();

    public ComponentReference<MovementCategory.MovementCategory>? CategoryReference  
        => GetReferencesOfType<MovementCategory.MovementCategory>().FirstOrDefault();

    public ComponentReference<MusclesGroup>? MusclesGroupReference  
        => GetReferencesOfType<MusclesGroup>().FirstOrDefault();

    public ComponentReference<EquipmentList>? EquipmentListReference  
        => GetReferencesOfType<EquipmentList>().FirstOrDefault();
    
    public ComponentReference<MusclesGroup>? MusclesWorkedReference
        => GetReferencesOfType<MusclesGroup>().FirstOrDefault();

    public MusclesGroup MusclesesWorked =>
        HasMuscleGroup ? MusclesWorkedReference!.Component! : new MusclesGroup();
    
    public Movement? VariantOf 
        => HasMovementVariant ? VariantOfReference!.Component : null;

    public EquipmentList RequiredEquipment 
        => RequireEquipment ? EquipmentListReference!.Component! : new EquipmentList();

    public WorkloadCalculationProfile? WorkloadCalculation
        => WorkloadProfileReference != null ? WorkloadProfileReference.Component :new DefaultWorkloadCalculationProfile();
   
    public ComponentReference<WorkloadCalculationProfile>? WorkloadProfileReference =>
        GetReferencesOfType<WorkloadCalculationProfile>().FirstOrDefault();

   

    #endregion

    #region Add Components

    public void AddWorkloadProfile(WorkloadCalculationProfile profile) =>
        Add(eWorkoutComponents.WorkloadProfile, profile);
    
    public void AddVariantOf(Movement variant) 
        => Add(eWorkoutComponents.Movement, variant);
    
    public void AddMuscleGroups(MusclesGroup musclesGroup) 
        =>  Add(eWorkoutComponents.MuscleGroup, musclesGroup);
    
    public void AddEquipmentList(EquipmentList equipments) 
        =>  Add(eWorkoutComponents.EquipmentList, equipments);

    
    public void AddMainMuscles(MuscleList muscleList)
    {
       // Console.WriteLine($"[AddMainMuscles] Called with {muscleList.Count} muscles");

        var musclegroup = HasMuscleGroup ? MusclesesWorked : new MusclesGroup();
      //  Console.WriteLine($"[AddMainMuscles] Using {(HasMuscleGroup ? "existing" : "new")} MuscleGroups");

        musclegroup.AddMainMuscles(muscleList);
     //   Console.WriteLine("[AddMainMuscles] Added muscles to main group");

        if (!HasMuscleGroup)
        {
            AddMuscleGroups(musclegroup);
          //  Console.WriteLine("[AddMainMuscles] Added new MuscleGroups to Content");
        }
    }

    public void AddSecondaryMuscles(MuscleList secondaryMuscles)
    {
     //   Console.WriteLine($"[AddSecondaryMuscles] Called with {secondaryMuscles.Count} muscles");

        var musclegroup = HasMuscleGroup ? MusclesesWorked : new MusclesGroup();
     //   Console.WriteLine($"[AddSecondaryMuscles] Using {(HasMuscleGroup ? "existing" : "new")} MuscleGroups");

        musclegroup.AddSecondaryMuscles(secondaryMuscles);
      //  Console.WriteLine("[AddSecondaryMuscles] Added muscles to secondary group");

        if (!HasMuscleGroup)
        {
            AddMuscleGroups(musclegroup);
          //  Console.WriteLine("[AddSecondaryMuscles] Added new MuscleGroups to Content");
        }
    }
    
    

    public void AddEquipment(Equipment.Equipment equipment)
        => RequiredEquipment.Items.Add(ComponentReference<Equipment.Equipment>.FromComponent(equipment));
    
    
    public void AddMovementCategory(GainsLab.Models.WorkoutComponents.MovementCategory.MovementCategory category) =>
        Add(eWorkoutComponents.MovementCategory, category);

  

    #endregion
 
    
    public override string ToString()
    {
        string variant = HasMovementVariant ? VariantOf?.Descriptor?.Name ?? "Unnamed" : "None";
        string category = CategoryReference?.Identifier?.ToString() ?? "None";
      //  string equipment = RequireEquipment ?RequiredEquipment.ToString() : "None";

        return $"MovementContent:\n  VariantOf: {variant}\n  Muscles: [{MusclesesWorked}]\n  Category: {category}\n  Equipment: {RequiredEquipment}  Calculation Profile {WorkloadCalculation}";
    }


  
}