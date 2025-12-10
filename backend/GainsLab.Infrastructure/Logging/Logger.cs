using GainsLab.Domain.Interfaces;

namespace GainsLab.Infrastructure.Logging;

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
    public GainsLabLogger(string name, DecoratorColor colorHeader)
    {
        LoggerName = name;
        _colorHeader = colorHeader;
        _decorate = _colorHeader != DecoratorColor.None;
    }

    public enum DecoratorColorContext
    {
        Reset,
        Info,
        Context,
       Header,
        WarningText,
       WarningHeader,
        ErrorText,
       ErrorHeader,
       None
    }
    
    public enum DecoratorColor
    {
       White,
       Cyan,
       Green,
       Yellow,
       Red,
       Reset,
       None,
    }

    private bool _enabled = true;
    /// <inheritdoc />
    public bool Enabled => _enabled;

    private bool _decorate = true;

    
    /// <summary>
    /// Gets the display name of the logger.
    /// </summary>
    public readonly string LoggerName = "Logger";

    private readonly DecoratorColor _colorHeader = DecoratorColor.Green;


    public string GetColor( DecoratorColorContext colorContext )
    {
        if (!_decorate) colorContext = DecoratorColorContext.None;
        
        return colorContext switch
        {
            DecoratorColorContext.Reset => GetColorString(DecoratorColor.Reset),
            DecoratorColorContext.Info => GetColorString(DecoratorColor.White),
            DecoratorColorContext.Context => GetColorString(DecoratorColor.Cyan),
            DecoratorColorContext.Header =>  GetColorString(_colorHeader),
            DecoratorColorContext.WarningText => GetColorString(DecoratorColor.Yellow),
            DecoratorColorContext.WarningHeader => GetColorString(DecoratorColor.Yellow),
            DecoratorColorContext.ErrorText => GetColorString(DecoratorColor.Red),
            DecoratorColorContext.ErrorHeader =>  GetColorString(DecoratorColor.Red),
            _ =>  GetColorString(DecoratorColor.None),
        };
        
    }
    
    public string GetColorString(DecoratorColor color)
    {
        return color switch
        {
            DecoratorColor.White =>"\u001b[37m",
            DecoratorColor.Cyan =>  "\u001b[36m",
            DecoratorColor.Green => "\u001b[32m",
            DecoratorColor.Yellow =>  "\u001b[33m",
            DecoratorColor.Red => "\u001b[31m",
            DecoratorColor.Reset => "\u001b[0m",
            DecoratorColor.None => "",
            _ => ""
        };
    }

    private string ColorReset => GetColor(DecoratorColorContext.Reset); //"\u001b[0m";
    private string ColorInfo  => GetColor(DecoratorColorContext.Info); // "\u001b[37m";   // White
    private string ColorContext  => GetColor(DecoratorColorContext.Context); //"\u001b[36m"; // Cyan
    private string ColorHeader => GetColor(DecoratorColorContext.Header); // "\u001b[32m"; // Green
    private string ColorWarningText  => GetColor(DecoratorColorContext.WarningText); // "\u001b[33m"; // Yellow
    private  string ColorWarningHeader => GetColor(DecoratorColorContext.WarningHeader); // "\u001b[33m"; // Yellow
    private  string ColorErrorText => GetColor(DecoratorColorContext.ErrorText); // "\u001b[31m";   // Red
    private string ColorErrorHeader => GetColor(DecoratorColorContext.ErrorHeader); // "\u001b[31m";   // Red

    public void ToggleDecoration(bool state)
    {
        _decorate = state;
    }

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
