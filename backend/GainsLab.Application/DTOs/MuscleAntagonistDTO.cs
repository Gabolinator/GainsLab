namespace GainsLab.Application.DTOs;

/// <summary>
/// Join entity representing a muscle-antagonist relationship.
/// </summary>
public class MuscleAntagonistRecord
{
    public int MuscleId { get; set; }

    public MuscleRecord Muscle { get; set; } = null!;

    public int AntagonistId { get; set; }

    public MuscleRecord Antagonist { get; set; } = null!;
}
