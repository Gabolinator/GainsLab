namespace GainsLab.Infrastructure.DB.DTOs;

/// <summary>
/// Join entity connecting movements to the equipment they require.
/// </summary>
public class MovementEquipmentRelationDTO
{
    public int MovementId { get; set; }

    public MovementDTO Movement { get; set; } = null!;

    public int EquipmentId { get; set; }
    
    public EquipmentDTO Equipment { get; set; } = null!;
}
