using GainsLab.Core.Models.Core;
using GainsLab.Models.Core;
using GainsLab.Models.Core.Descriptor;
using GainsLab.Models.Core.Interfaces;

namespace GainsLab.Models.WorkoutComponents.Muscle;

// public class Muscle : ICompositeWorkoutComponent<MuscleContent>
// {
//
//     public Muscle()
//     {
//     }
//     
//
//     public Muscle(ComponentDescriptor descriptor, WorkoutComponentIdentifier identifier, eBodySection bodySection, Muscle? antagonist)
//     {
//         Descriptor = descriptor;
//         Identifier = identifier;
//         BodySection = bodySection;
//         Content = new MuscleContent();
//         SetAntagonist(antagonist);
//         
//     }
//
//     public Muscle(string name, string stringID, eBodySection bodySection ,Muscle? antagonist = null)
//     {
//         Descriptor = new ComponentDescriptor(name, ComponentType);
//         Identifier = new WorkoutComponentIdentifier(stringID, ComponentType);
//         BodySection = bodySection;
//         Content = new MuscleContent();
//         SetAntagonist(antagonist);
//        
//     }
//     
//     public eWorkoutComponents ComponentType => eWorkoutComponents.Muscle;
//     
//     public ComponentDescriptor Descriptor { get; set; }
//     public WorkoutComponentIdentifier Identifier { get; set; }
//     public MediaInfos Medias { get; set; }
//     public MuscleContent Content { get; set  ; } 
//     
//     /*--Specific members--*/
//     public eBodySection BodySection { get; set; } = eBodySection.undefined;
//
//     public Muscle? Antagonist => Content.GetAntagonist(); 
//     
//     public void SetAntagonist(Muscle? antagonist)
//     {
//         Content = antagonist != null? new MuscleContent(eWorkoutComponents.Muscle, antagonist) : new MuscleContent();
//     }
//
//     public static void SetAsAntagonists(Muscle  muscle1, Muscle muscle2)
//     {
//         muscle1.SetAntagonist(muscle2);
//         muscle2.SetAntagonist(muscle1);
//     }
//     
//     public IWorkoutComponent Copy()
//     {
//         return new Muscle((ComponentDescriptor)Descriptor.Copy(),(WorkoutComponentIdentifier)Identifier.Copy(), BodySection, Antagonist );
//         
//     }
//     
//     public override string ToString()
//     {
//         return $"Muscle: \"{Descriptor?.Name ?? "Unnamed"}\", ID: [{Identifier}], Section: {BodySection}, Antagonist: [{Content}]";
//     }
// }