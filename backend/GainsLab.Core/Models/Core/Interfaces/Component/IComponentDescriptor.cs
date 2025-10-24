using System;
using System.Net.Mime;
using GainsLab.Core.Models.Core;
using GainsLab.Core.Models.Core.Descriptor;
using GainsLab.Models.Core.Descriptor;

namespace GainsLab.Models.Core.Interfaces;

// OUTDATED: Legacy IComponentDescriptor contract retained for later refactor.

// public interface IComponentDescriptor
// {
//     string Name { get; set; }
//     public Notes Notes { get; set; }
//     public Description Description { get; set; }
//     public WorkoutComponentCreationInfo CreationInfo { get; set; }
//     
//     public MediaInfos Medias { get; set; }
//
//     public bool IsEmpty() => Notes.IsEmpty() && 
//                              Description.IsEmpty() && 
//                              CreationInfo.IsEmpty() &&
//                              string.IsNullOrWhiteSpace(Name);
//
//     public IComponentDescriptor Copy();
//    
//     
//     
//     public string ToString();
// }



