using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GainsLab.Core.Models.Core;

namespace GainsLab.Infrastructure.DB.DTOs;

[Table("descriptors")]
public class DescriptorDTO : BaseDto
{
    [Key]
    public int Id { get; set; }

    public Guid GUID { get; set; }

    public string Content { get; set; } = "none";

    public override string? GetContent() => Content;
   

    public override int Iid => Id;
    public override Guid Iguid => GUID;
    public override EntityType Type => EntityType.Descriptor;

    public override string ToString()
    {
        return $"{nameof(DescriptorDTO)} - " +
               $"ID : {Id} | " +
               $"Guid :{GUID} |"+
               $"Content :{Content} ";
    }
}