using System.Linq;
using GainsLab.Models.Core;

namespace GainsLab.Models.WorkoutComponents.Muscle;

// public class MuscleContent :  ComponentContent
// {
//     public MuscleContent()
//     {
//     }
//
//     public  MuscleContent(eWorkoutComponents type, Muscle? component)
//     {
//         if(component == null) return;
//         
//         Add(type, component);
//     }
//
//
//
//     public bool HasAntagonist => AntagonistMuscleReference != null && AntagonistMuscleReference.IsComponentResolved;
//
//     public ComponentReference<Muscle>? AntagonistMuscleReference => GetReferencesOfType<Muscle>().FirstOrDefault();
//     
//     // public bool TryGetAntagonistMuscleFromRef(out Muscle? muscle)
//     // {
//     //     muscle = GetResolvedComponentsOfType<Muscle>().FirstOrDefault();
//     //     return muscle == null;
//     // }
//     //
//     // public bool TryGetAntagonistMuscleFromId(out Muscle? muscle)
//     // {
//     //     muscle = null;
//     //     
//     //     var id = GetIdsOfType<Muscle>().FirstOrDefault();
//     //     if (id == null) return false;
//     //
//     //    return DataManager.TryGetComponentFromId<Muscle>(id, out muscle);
//     // }
//
//     public Muscle? GetAntagonist() => HasAntagonist ? AntagonistMuscleReference!.Component : null;
//   
//     
//     public void SetAntagonist(Muscle? antagonist)
//     {
//        if(antagonist == null) return;
//        Add(eWorkoutComponents.Muscle, antagonist.Identifier);
//     }
//     
//     public override string ToString()
//     {
//         string variant =  HasAntagonist? GetAntagonist()?.Descriptor?.Name ?? "Unnamed" : "None";
//         return $"MuscleContent:\n  Antagonist: {variant}";
//     }
//     
// }