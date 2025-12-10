using GainsLab.Domain;

namespace GainsLab.Application.DTOs;

/// <summary>
/// Join entity linking a movement to a muscle and the role that muscle plays.
/// </summary>
public class MovementMuscleRelationRecord
{
    public int MovementId { get; set; }

    public MovementRecord Movement { get; set; } = null!;

    public int MuscleId { get; set; }
    
    public MuscleRecord Muscle { get; set; } = null!;

    public MuscleRole MuscleRole { get; set; }
}
