using GainsLab.Domain.Entities.Identifier;

namespace GainsLab.Domain.Entities.WorkoutEntity;

/// <summary>
/// Describes the primary and secondary muscles engaged by a movement.
/// </summary>
public class MuscleWorked
{

    public MuscleWorked()
    {
    }
    public MuscleWorked(IEnumerable<MuscleEntity> main, IEnumerable<MuscleEntity> secondary)
    {
        PrimaryMuscles = new MuscleIdList(main.Select(m => m.Id));
        SecondaryMuscles = new MuscleIdList(secondary.Select(m => m.Id));
    }
    

    public MuscleIdList PrimaryMuscles { get; set; }
    public MuscleIdList SecondaryMuscles { get; set; }

    private MuscleIdList? AllMuscles { get; set; } = null;
    
    public MuscleIdList GetAllMuscle()
    {
       return AllMuscles ??= new MuscleIdList(ContatMuscles()) ;
        
    }

    private IEnumerable<MuscleId> ContatMuscles()
    {
        var list = new MuscleIdList(PrimaryMuscles);
        list.AddUniques(SecondaryMuscles);
        return list;
    }
}