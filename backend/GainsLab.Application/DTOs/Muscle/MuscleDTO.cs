using System.ComponentModel.DataAnnotations;
using GainsLab.Application.DTOs.Description;
using GainsLab.Domain;

namespace GainsLab.Application.DTOs.Muscle;

/// <summary>
/// Database representation of a muscle entity along with descriptor and antagonists.
/// </summary>
public class MuscleRecord : BaseRecord
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public Guid GUID { get; set; }

    public int DescriptorID { get; set; }

    public DescriptorRecord? Descriptor { get; set; }

    public eBodySection BodySection { get; set; } = eBodySection.undefined;

    //antagonist of this muscles
    public ICollection<MuscleAntagonistRecord> Antagonists { get; set; } = new List<MuscleAntagonistRecord>();

    //to which other muscles is this muscle considered antagonist
    public ICollection<MuscleAntagonistRecord> Agonists { get; set; } = new List<MuscleAntagonistRecord>();

    public override string? GetContent() => Name;

    public override int Iid => Id;

    public override Guid Iguid => GUID;

    public override EntityType Type => EntityType.Muscle;

    /// <summary>
    /// Convenience accessor used by domain mappers.
    /// </summary>
    public IEnumerable<Guid> AntagonistGUIDs => Antagonists.Select(link => link.Antagonist.GUID).Distinct();
}
