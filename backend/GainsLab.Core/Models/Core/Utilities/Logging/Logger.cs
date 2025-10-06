using System.Runtime.CompilerServices;
using GainsLab.Models.Logging;

namespace GainsLab.Core.Models.Core.Utilities.Logging;

public class GainsLabLogger : ILogger
{
    public GainsLabLogger()
    {
        
    }

    public GainsLabLogger(string name)
    {
        LoggerName = name;
    }
    
    public GainsLabLogger(string name, string colorHeader)
    {
        LoggerName = name;
        ColorHeader = colorHeader;
    }

    private bool _enabled = true;
    public bool Enabled => _enabled;

    public readonly string LoggerName = "Logger";
    
    private const string ColorReset = "\u001b[0m";
    private const string ColorInfo = "\u001b[37m";   // White
    private const string ColorContext = "\u001b[36m"; // Cyan
    private readonly string ColorHeader = "\u001b[32m"; // Green
    private const string ColorWarningText = "\u001b[33m"; // Yellow
    private const string ColorWarningHeader = "\u001b[33m"; // Yellow
    private const string ColorErrorText = "\u001b[31m";   // Red
    private const string ColorErrorHeader = "\u001b[31m";   // Red
    
    public void ToggleLogging(bool state)
    {
        Console.WriteLine($"{ColorHeader}[{LoggerName}] Toggled logging {state}");
        _enabled = state;
    }

    public void Log(string message)
    {
        if (!Enabled) return;

        Console.WriteLine($"{ColorHeader}[{LoggerName}] {ColorInfo}{message}{ColorReset}");
    }

    public void Log(string context, string message)
    {
        if (!Enabled) return;

        Console.WriteLine($"{ColorHeader}[{LoggerName}] - {ColorContext}[{context}] {ColorInfo}{message}{ColorReset}");
    }

    public void LogWarning(string context, string message)
    {
        if (!Enabled) return;

        Console.WriteLine($"{ColorWarningHeader}[{LoggerName}]-WARNING - {ColorContext}[{context}] {ColorWarningText}{message}{ColorReset}");
    }

    public void LogError(string context, string message)
    {
        if (!Enabled) return;

        Console.WriteLine($"{ColorErrorHeader}[{LoggerName}]-ERROR -  {ColorContext}[{context}] {ColorErrorText}{message}{ColorReset}");
    }
}