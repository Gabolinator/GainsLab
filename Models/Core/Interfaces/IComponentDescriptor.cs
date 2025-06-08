using System;
using System.Net.Mime;

namespace GainsLab.Models.Core;

public interface IComponentDescriptor
{
    string Name { get; set; }
    public Notes Notes { get; set; }
    public Description Description { get; set; }
    public CreationInfo CreationInfo { get; set; }

    public bool IsEmpty() => Notes.IsEmpty() && 
                             Description.IsEmpty() && 
                             CreationInfo.IsEmpty() &&
                             string.IsNullOrWhiteSpace(Name);

    public IComponentDescriptor Copy();
   
    
    
    public string ToString();
}



