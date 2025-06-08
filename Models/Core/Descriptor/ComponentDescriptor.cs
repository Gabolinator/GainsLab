namespace GainsLab.Models.Core;

public class ComponentDescriptor : IComponentDescriptor
{
    public ComponentDescriptor(string name, Notes notes, Description description, CreationInfo creationInfo)
    {
        Name = name;
        Notes = notes;
        Description = description;
        CreationInfo = creationInfo;
        Tags  = new TagList();
    }

    public ComponentDescriptor(string name, eWorkoutComponents type)
    {
        Name = name;
        Notes = new EmptyNotes();
        Description = new EmptyDescription();
        CreationInfo = new DefaultCreationInfo(type);
        Tags  = new TagList();
    }


    public string Name { get; set; }
    public Notes Notes { get; set; }
    public Description Description { get; set; }
    public CreationInfo CreationInfo { get; set; }
    public TagList Tags { get; set; }

    public IComponentDescriptor Copy()
    {
       var comp = new ComponentDescriptor(
           Name, 
           Notes.Copy(),
           Description.Copy(), 
           CreationInfo.Copy());

       comp.Tags = Tags.Copy();

       return comp;
    }

    public override string ToString()
    {
        return $"ComponentDescriptor: \"{Name}\"\n"
               + $"{Notes}\n"
               + $"{Description}\n"
               + $"{CreationInfo}\n"
               + $"{Tags}";
    }



}

public class EmptyDescriptor() : ComponentDescriptor("", eWorkoutComponents.unidentified);