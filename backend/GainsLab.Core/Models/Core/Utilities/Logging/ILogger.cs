namespace GainsLab.Core.Models.Core.Utilities.Logging;

/// <summary>
/// Logging abstraction used throughout GainsLab services.
/// </summary>
public interface ILogger
{
    /// <summary>
    /// Gets a value indicating whether logging is currently enabled.
    /// </summary>
    bool Enabled { get;}

    /// <summary>
    /// Enable or disable color decoration
    /// </summary>
    /// <param name="state"> to toggle the decoration</param>
    /// <returns></returns>
    void ToggleDecoration(bool state);

    /// <summary>
    /// Enables or disables the logger.
    /// </summary>
    /// <param name="state"><c>true</c> to enable logging; <c>false</c> to suppress output.</param>
    void ToggleLogging(bool state);
    
    /// <summary>
    /// Writes an informational message without context.
    /// </summary>
    /// <param name="message">The message to emit.</param>
    void Log(string message);
    
    /// <summary>
    /// Writes an informational message including a context label.
    /// </summary>
    /// <param name="context">Logical context describing the source of the message.</param>
    /// <param name="message">The message to emit.</param>
    void Log(string context,string message);
    
    /// <summary>
    /// Writes a warning message associated with the supplied context.
    /// </summary>
    /// <param name="context">Logical context describing the source of the warning.</param>
    /// <param name="message">The warning message to emit.</param>
    void LogWarning(string context,string message);
    
    /// <summary>
    /// Writes an error message associated with the supplied context.
    /// </summary>
    /// <param name="context">Logical context describing the source of the error.</param>
    /// <param name="message">The error message to emit.</param>
    void LogError(string context,string message);
}
