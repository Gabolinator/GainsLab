using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GainsLab.Models.DataManagement.DB.Model.DTOs;

[Table("Descriptors")]
public class DescriptorDTO : IDto
{
    [Key]
    public int Id { get; set; }

    public Guid GUID { get; set; }
    public int Iid => Id;
    public Guid Iguid => GUID;
}