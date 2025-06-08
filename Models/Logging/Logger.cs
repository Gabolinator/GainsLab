using System;

namespace GainsLab.Models.Logging;

public class WorkoutLogger : IWorkoutLogger
{
    private bool _enabled = true;
    public bool Enabled => _enabled;

    public void ToggleLogging(bool state)
    {
        Console.WriteLine($"[WorkoutLog] Toggled logging {state}");
        _enabled = state;
    }

    public void Log(string message)
    {
        if(!Enabled) return;
        Console.WriteLine($"[WorkoutLog] {message}");
    }
    
    public void Log(string context,string message)
    {
        if(!Enabled) return;
        Console.WriteLine($"[{context}] {message}");
    }
}