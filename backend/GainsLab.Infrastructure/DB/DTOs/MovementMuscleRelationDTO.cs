using GainsLab.Core.Models.Core;

namespace GainsLab.Infrastructure.DB.DTOs;

/// <summary>
/// Join entity linking a movement to a muscle and the role that muscle plays.
/// </summary>
public class MovementMuscleRelationDTO
{
    public int MovementId { get; set; }

    public MovementDTO Movement { get; set; } = null!;

    public int MuscleId { get; set; }
    
    public MuscleDTO Muscle { get; set; } = null!;

    public MuscleRole MuscleRole { get; set; }
}
