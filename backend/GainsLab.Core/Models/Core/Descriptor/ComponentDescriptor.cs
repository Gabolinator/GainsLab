using System.ComponentModel.DataAnnotations.Schema;
using GainsLab.Models.Core.Interfaces;

namespace GainsLab.Models.Core.Descriptor;


// OUTDATED: Legacy ComponentDescriptor implementation retained for later refactor.
// /// <summary>
// /// Represents a descriptor for a workout component, containing metadata such as name, notes, description, creation info, and tags.
// /// </summary>
// public class ComponentDescriptor : IComponentDescriptor
// {
//
//     /// <summary>
//     /// Initializes a new instance of the <see cref="ComponentDescriptor"/> class.
//     /// </summary>
//     public ComponentDescriptor()
//     {
//         
//     }
//     
//     /// <summary>
//     /// Initializes a new instance of the <see cref="ComponentDescriptor"/> class with detailed information.
//     /// </summary>
//     /// <param name="name">The name of the component.</param>
//     /// <param name="notes">The notes associated with the component.</param>
//     /// <param name="description">The description of the component.</param>
//     /// <param name="creationInfo">The creation info for the component.</param>
//     public ComponentDescriptor(string name, Notes notes, Description description, WorkoutComponentCreationInfo creationInfo)
//     {
//         Name = name;
//         Notes = notes;
//         Description = description;
//         CreationInfo = creationInfo;
//         Tags  = new TagList();
//     }
//
//     
//     /// <summary>
//     /// Initializes a new instance of the <see cref="ComponentDescriptor"/> class with a name and component type.
//     /// </summary>
//     /// <param name="name">The name of the component.</param>
//     /// <param name="type">The type of workout component.</param>
//     public ComponentDescriptor(string name, eWorkoutComponents type)
//     {
//         Name = name;
//         Notes = new EmptyNotes();
//         Description = new EmptyDescription();
//         CreationInfo = new DefaultWorkoutComponentCreationInfo(type);
//         Tags  = new TagList();
//     }
//
//     
//     /// <summary>
//     /// Gets or sets the unique identifier of the Descriptor component.
//     /// </summary>
//     public int Id { get; set; }
//
//     /// <summary>
//     /// Gets or sets the name of the Descriptor component.
//     /// </summary>
//     public string Name { get; set; }
//     
//     /// <summary>
//     /// Gets or sets the notes associated with the component.
//     /// </summary>
//     public Notes Notes { get; set; }
//     
//     /// <summary>
//     /// Gets or sets the description of the component.
//     /// </summary>
//     public Description Description { get; set; }
//     
//     /// <summary>
//     /// Gets or sets the creation info of the component.
//     /// </summary>
//     public WorkoutComponentCreationInfo CreationInfo { get; set; }
//
//     public MediaInfos Medias { get; set; }
//
//     /// <summary>
//     /// Gets or sets the list of tags associated with the component. This property is not mapped to the database.
//     /// </summary>
//     [NotMapped]
//     public TagList Tags { get; set; }
//
//     
//     /// <summary>
//     /// Creates a deep copy of the current component descriptor.
//     /// </summary>
//     /// <returns>A copy of the component descriptor.</returns>
//     public IComponentDescriptor Copy()
//     {
//        var comp = new ComponentDescriptor(
//            Name, 
//            Notes.Copy(),
//            Description.Copy(), 
//            CreationInfo.Copy());
//
//        comp.Tags = Tags.Copy();
//
//        return comp;
//     }
//
//     /// <summary>
//     /// Returns a string representation of the component descriptor.
//     /// </summary>
//     /// <returns>A string describing the component descriptor.</returns>
//     public override string ToString()
//     {
//         return $"ComponentDescriptor: \"{Name}\"\n"
//                + $"{Notes}\n"
//                + $"{Description}\n"
//                + $"{CreationInfo}\n"
//                + $"{Tags}";
//     }
//
//
//
// }
//
// /// <summary>
// /// Represents an empty descriptor for unidentified workout components.
// /// </summary>
// public class EmptyDescriptor() : ComponentDescriptor("", eWorkoutComponents.unidentified);
