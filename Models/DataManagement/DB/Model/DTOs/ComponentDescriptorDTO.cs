using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GainsLab.Models.DataManagement.DB.Model.DTOs;

[Table("Descriptors")]
public class ComponentDescriptorDTO
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; }
}