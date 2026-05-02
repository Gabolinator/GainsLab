using GainsLab.Domain.Interfaces.Entity;

namespace GainsLab.Domain.Entities.WorkoutEntity.EntityContent;

/// <summary>
/// Mutable content describing a muscle and additional metadata.
/// </summary>
public class MuscleContent : IEntityContent<MuscleContent>
{
    public string Name { get; set; }

    public string LatinName { get; set; }
    public eBodySection BodySection { get; set; } = eBodySection.undefined;
   
    public MuscleContent Validate()
    {
        if (string.IsNullOrWhiteSpace(Name)) throw new ArgumentException("Muscle name is required.", nameof(Name));
        // if (string.IsNullOrWhiteSpace(LatinName)) throw new ArgumentException("Latin name is required.", nameof(LatinName));
        if (BodySection == eBodySection.undefined) throw new ArgumentException("Body section must be specified.", nameof(BodySection));
        return this;
    }
}