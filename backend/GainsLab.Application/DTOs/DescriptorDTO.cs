using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GainsLab.Contracts.Dtos.PutDto;
using GainsLab.Domain;

namespace GainsLab.Application.DTOs;

[Table("descriptors")]
public class DescriptorRecord : BaseRecord
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
        return $"{nameof(DescriptorRecord)} - " +
               $"ID : {Id} | " +
               $"Guid :{GUID} |"+
               $"Content :{Content} ";
    }

  
}