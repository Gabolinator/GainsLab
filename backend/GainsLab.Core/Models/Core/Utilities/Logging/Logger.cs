using GainsLab.Models.Logging;

namespace GainsLab.Core.Models.Logging;

public class WorkoutLogger : ILogger
{
    private bool _enabled = true;
    public bool Enabled => _enabled;

    
    private const string ColorReset = "\u001b[0m";
    private const string ColorInfo = "\u001b[37m";   // White
    private const string ColorContext = "\u001b[36m"; // Cyan
    private const string ColorWarningText = "\u001b[33m"; // Yellow
    private const string ColorWarningHeader = "\u001b[33m"; // Yellow
    private const string ColorErrorText = "\u001b[31m";   // Red
    private const string ColorErrorHeader = "\u001b[31m";   // Red
    
    public void ToggleLogging(bool state)
    {
        Console.WriteLine($"[WorkoutLog] Toggled logging {state}");
        _enabled = state;
    }

    public void Log(string message)
    {
        if (!Enabled) return;

        Console.WriteLine($"{ColorContext}[WorkoutLog] {ColorInfo}{message}{ColorReset}");
    }

    public void Log(string context, string message)
    {
        if (!Enabled) return;

        Console.WriteLine($"{ColorContext}[{context}] {ColorInfo}{message}{ColorReset}");
    }

    public void LogWarning(string context, string message)
    {
        if (!Enabled) return;

        Console.WriteLine($"{ColorWarningHeader}WARNING - {ColorContext}[{context}] {ColorWarningText}{message}{ColorReset}");
    }

    public void LogError(string context, string message)
    {
        if (!Enabled) return;

        Console.WriteLine($"{ColorErrorHeader}ERROR -  {ColorContext}[{context}] {ColorErrorText}{message}{ColorReset}");
    }
}