using GainsLab.Application.DTOs.Equipment;

namespace GainsLab.Application.DTOs.Movement;

/// <summary>
/// Join entity connecting movements to the equipment they require.
/// </summary>
public class MovementEquipmentRelationRecord
{
    public int MovementId { get; set; }

    public MovementRecord Movement { get; set; } = null!;

    public int EquipmentId { get; set; }
    
    public EquipmentRecord Equipment { get; set; } = null!;
}
