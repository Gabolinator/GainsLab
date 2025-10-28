using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GainsLab.Core.Models.Core.Interfaces.DB;
using GainsLab.Infrastructure.DB.DTOs;

namespace GainsLab.Models.DataManagement.DB.Model.DTOs;

[Table("descriptors")]
public class DescriptorDTO : BaseDto
{
    [Key]
    public int Id { get; set; }

    public Guid GUID { get; set; }

    public string Content { get; set; } = "none";

    public override int Iid => Id;
    public override Guid Iguid => GUID;

    public override string ToString()
    {
        return $"{nameof(DescriptorDTO)} - " +
               $"ID : {Id} | " +
               $"Guid :{GUID} |"+
               $"Content :{Content} ";
    }
}