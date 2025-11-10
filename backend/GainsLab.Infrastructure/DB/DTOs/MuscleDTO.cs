using System.ComponentModel.DataAnnotations;
using System.Linq;
using GainsLab.Core.Models.Core;
using GainsLab.Core.Models.Core.Entities.Identifier;

namespace GainsLab.Infrastructure.DB.DTOs;

/// <summary>
/// Database representation of a muscle entity along with descriptor and antagonists.
/// </summary>
public class MuscleDTO : BaseDto
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public Guid GUID { get; set; }

    public int DescriptorID { get; set; }

    public DescriptorDTO? Descriptor { get; set; }

    public eBodySection BodySection { get; set; } = eBodySection.undefined;

    public ICollection<MuscleAntagonistDTO> Antagonists { get; set; } = new List<MuscleAntagonistDTO>();

    public ICollection<MuscleAntagonistDTO> Agonists { get; set; } = new List<MuscleAntagonistDTO>();

    public override string? GetContent() => Name;

    public override int Iid => Id;

    public override Guid Iguid => GUID;

    public override EntityType Type => EntityType.Muscle;

    /// <summary>
    /// Convenience accessor used by domain mappers.
    /// </summary>
    public IEnumerable<Guid> AntagonistGUIDs => Antagonists.Select(link => link.Antagonist.GUID).Distinct();
}
