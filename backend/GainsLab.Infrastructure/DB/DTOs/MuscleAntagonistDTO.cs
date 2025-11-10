namespace GainsLab.Infrastructure.DB.DTOs;

/// <summary>
/// Join entity representing a muscle-antagonist relationship.
/// </summary>
public class MuscleAntagonistDTO
{
    public int MuscleId { get; set; }

    public MuscleDTO Muscle { get; set; } = null!;

    public int AntagonistId { get; set; }

    public MuscleDTO Antagonist { get; set; } = null!;
}
