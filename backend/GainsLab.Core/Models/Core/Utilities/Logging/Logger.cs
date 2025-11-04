using System.Runtime.CompilerServices;

namespace GainsLab.Core.Models.Core.Utilities.Logging;

/// <summary>
/// Console-based logger with colored output for GainsLab components.
/// </summary>
public class GainsLabLogger : ILogger
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GainsLabLogger"/> class using the default name.
    /// </summary>
    public GainsLabLogger()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GainsLabLogger"/> class with a custom name.
    /// </summary>
    /// <param name="name">The label displayed in logging output.</param>
    public GainsLabLogger(string name)
    {
        LoggerName = name;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="GainsLabLogger"/> class with custom name and color.
    /// </summary>
    /// <param name="name">The label displayed in logging output.</param>
    /// <param name="colorHeader">ANSI color code applied to the logger header.</param>
    public GainsLabLogger(string name, string colorHeader)
    {
        LoggerName = name;
        ColorHeader = colorHeader;
    }

    private bool _enabled = true;
    /// <inheritdoc />
    public bool Enabled => _enabled;

    /// <summary>
    /// Gets the display name of the logger.
    /// </summary>
    public readonly string LoggerName = "Logger";
    
    private const string ColorReset = "\u001b[0m";
    private const string ColorInfo = "\u001b[37m";   // White
    private const string ColorContext = "\u001b[36m"; // Cyan
    private readonly string ColorHeader = "\u001b[32m"; // Green
    private const string ColorWarningText = "\u001b[33m"; // Yellow
    private const string ColorWarningHeader = "\u001b[33m"; // Yellow
    private const string ColorErrorText = "\u001b[31m";   // Red
    private const string ColorErrorHeader = "\u001b[31m";   // Red
    
    /// <inheritdoc />
    public void ToggleLogging(bool state)
    {
        Console.WriteLine($"{ColorHeader}[{LoggerName}] Toggled logging {state}");
        _enabled = state;
    }

    /// <inheritdoc />
    public void Log(string message)
    {
        if (!Enabled) return;

        Console.WriteLine($"{ColorHeader}[{LoggerName}] {ColorInfo}{message}{ColorReset}");
    }

    /// <inheritdoc />
    public void Log(string context, string message)
    {
        if (!Enabled) return;

        Console.WriteLine($"{ColorHeader}[{LoggerName}] - {ColorContext}[{context}] {ColorInfo}{message}{ColorReset}");
    }

    /// <inheritdoc />
    public void LogWarning(string context, string message)
    {
        if (!Enabled) return;

        Console.WriteLine($"{ColorWarningHeader}[{LoggerName}]-WARNING - {ColorContext}[{context}] {ColorWarningText}{message}{ColorReset}");
    }

    /// <inheritdoc />
    public void LogError(string context, string message)
    {
        if (!Enabled) return;

        Console.WriteLine($"{ColorErrorHeader}[{LoggerName}]-ERROR -  {ColorContext}[{context}] {ColorErrorText}{message}{ColorReset}");
    }
}
