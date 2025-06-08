using GainsLab.Models.Core;

namespace GainsLab.Models.WorkoutComponents;

public class WorkloadCalculationProfile : IWorkoutComponent
{
    
    public WorkloadCalculationProfile(IComponentDescriptor descriptor, IIdentifier identifier,  eWorkloadCalculationType calculationType)
    {
        Descriptor = descriptor;
        Identifier = identifier;
        CalculationType = calculationType;
    }
    
    public WorkloadCalculationProfile(string name, string stringID,  eWorkloadCalculationType calculationType)
    {
        Descriptor = new ComponentDescriptor(name,ComponentType);
        Identifier = new Identifier(stringID,ComponentType);
        CalculationType = calculationType;
    }
    
    public eWorkoutComponents ComponentType => eWorkoutComponents.WorkloadProfile;
    public IComponentDescriptor Descriptor { get; set; }
    public IIdentifier Identifier { get; set; }
    
    public eWorkloadCalculationType CalculationType { get; set; } = eWorkloadCalculationType.Custom;

    
    public IWorkoutComponent Copy()
    {
        throw new System.NotImplementedException();
    }
   
    // public bool UsesWeight { get; set; }
    // public bool UsesReps { get; set; }
    // public bool UsesTime { get; set; }
    // public bool UsesDistance { get; set; }
    
    public override string ToString()
    {
        return $"[WorkloadCalculationProfile] [ Descriptor: {Descriptor} ], " +
               $"[ ID: {Identifier} ], " +
               $"[ Type: {CalculationType} ]";
    }
    
}

public class DefaultWorkloadCalculationProfile(): WorkloadCalculationProfile("Default Workload Calculation Profile", "DefaultWCProfile", eWorkloadCalculationType.Weight_Reps)
{
    
    
}


public enum eWorkloadCalculationType
{
    Reps_Only,             // Bodyweight exercises
    Weight_Reps,           // Standard resistance training
    Weight_Time,           // Weighted isometrics
    Time_Only,             // Bodyweight holds (e.g. plank)
    Distance_Only,         // Running, cycling, swimming
    Weight_Distance,       // Rucking, weighted runs
    Calories,              // HIIT, ERG machines
    TTS,                    // Time To Set – good for tracking endurance-based sets
    RPE,                   // Subjective effort scale,
    HeartRate,
    Custom                 // Fallback for unique formulas
}
