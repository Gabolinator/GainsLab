namespace GainsLab.Models.Logging;

public interface IWorkoutLogger
{
    bool Enabled { get;}

    void ToggleLogging(bool state);
    
    void Log(string message);
    
    void Log(string context,string message);
    
    void LogWarning(string context,string message);
    
    void LogError(string context,string message);
}