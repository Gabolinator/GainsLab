using System;
using System.Net.Mime;
using GainsLab.Models.Core.Descriptor;

namespace GainsLab.Models.Core.Interfaces;

public interface IComponentDescriptor
{
    string Name { get; set; }
    public Notes Notes { get; set; }
    public Description Description { get; set; }
    public WorkoutComponentCreationInfo CreationInfo { get; set; }

    public bool IsEmpty() => Notes.IsEmpty() && 
                             Description.IsEmpty() && 
                             CreationInfo.IsEmpty() &&
                             string.IsNullOrWhiteSpace(Name);

    public IComponentDescriptor Copy();
   
    
    
    public string ToString();
}



