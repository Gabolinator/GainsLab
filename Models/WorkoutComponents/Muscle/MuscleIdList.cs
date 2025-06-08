using System.Collections.Generic;
using System.Linq;
using GainsLab.Models.DataManagement;

namespace GainsLab.Models.Core.WorkoutComponents;

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