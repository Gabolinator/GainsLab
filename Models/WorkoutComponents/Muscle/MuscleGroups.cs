
using System.Collections.Generic;
using GainsLab.Models.Core;
using GainsLab.Models.Core.Descriptor;
using GainsLab.Models.Core.Interfaces;

namespace GainsLab.Models.WorkoutComponents.Muscle;

public class MusclesGroup : IWorkoutComponent
{


    public MusclesGroup()
    {
    }
    
    public MusclesGroup(MuscleList mainMuscles, MuscleList secondaryMuscles)
    {
        AddMainMuscles(mainMuscles);
        AddSecondaryMuscles(secondaryMuscles);
    }
    
    public MusclesGroup(List<Muscle> mainMuscles, List<Muscle> secondaryMuscles)
    {
       AddMainMuscles(mainMuscles);
       AddSecondaryMuscles(secondaryMuscles);
    }

    public eWorkoutComponents ComponentType => eWorkoutComponents.MuscleGroup;
    public ComponentDescriptor Descriptor { get; set; } = new EmptyDescriptor();
    public Identifier Identifier { get; set; } = new EmptyIdentifier();
    public MuscleList MainMuscles { get; set; } = new();
    public MuscleList SecondaryMuscles { get; set; } = new();

    
    public void AddMainMuscles(MuscleList muscleList) => MainMuscles.AddComponents(muscleList);
    public void AddMainMuscles(List<Muscle> muscleList) => MainMuscles.AddComponents(muscleList);
    public void AddMainMuscle(Muscle muscle) => MainMuscles.AddComponent(muscle);
  
    public void AddSecondaryMuscle(Muscle muscle) => SecondaryMuscles.AddComponent(muscle);
    public void AddSecondaryMuscles(MuscleList muscleList) => SecondaryMuscles.AddComponents(muscleList);
    public void AddSecondaryMuscles(List<Muscle> muscleList) => SecondaryMuscles.AddComponents(muscleList);
 
    public void AddMainMuscles(List<IIdentifier> muscleList) => MainMuscles.AddComponents(muscleList);
    public void AddMainMuscle(IIdentifier muscle) => MainMuscles.AddComponent(muscle);
  
    public void AddSecondaryMuscle(IIdentifier muscle) => SecondaryMuscles.AddComponent(muscle);
    public void AddSecondaryMuscles(List<IIdentifier> muscleList) => SecondaryMuscles.AddComponents(muscleList);

    public MuscleList GetAllMuscles()
    {
        //Console.WriteLine("[MuscleGroups] Collecting all muscles (main + secondary)");

        var all = new MuscleList(MainMuscles);

       // Console.WriteLine($"[MuscleGroups] Main muscles count: {MainMuscles.Count}");
      //  Console.WriteLine($"[MuscleGroups] Secondary muscles count: {SecondaryMuscles.Count}");

        all.AddComponents(SecondaryMuscles);

        var distinct = all.GetDistinct();
      //  Console.WriteLine($"[MuscleGroups] Total distinct muscles returned: {distinct.Count}");

        return distinct;
    }

    public IWorkoutComponent Copy()
    {
        throw new System.NotImplementedException();
    }

    public override string ToString()
    {
        return $"MuscleGroups: Main[{MainMuscles}], Secondary[{SecondaryMuscles}], ID: [{Identifier}], Name: {Descriptor?.Name ?? "Unnamed"}";
    }




}