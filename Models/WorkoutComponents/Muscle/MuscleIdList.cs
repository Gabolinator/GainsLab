namespace GainsLab.Models.WorkoutComponents.Muscle;

// public class MuscleIdList : ComponentsIdentifierList
// {
//     public override eWorkoutComponents ComponentsType { get; protected set; } = eWorkoutComponents.Muscle;
//     
//     public MuscleIdList()
//     {
//      
//     }
//
//     public MuscleIdList(MuscleList list)
//     {
//         AddComponents(list.Select(it => it.Identifier).ToList());
//     }
//
//     public MuscleIdList(IEnumerable<Muscle> components)
//     {
//         AddComponents(components.Select(it => it.Identifier).ToList());
//     }
//     
//     public MuscleIdList(IEnumerable<IIdentifier> components)
//     {
//         AddComponents(components.ToList());
//     }
//
//     public MuscleIdList GetDistinct()
//     {
//         var distinct = this.Distinct().OfType<Identifier>().ToList();
//         return new MuscleIdList(distinct);
//     }

    
//}