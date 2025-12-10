namespace GainsLab.Domain.Interfaces;

/// <summary>
/// Provides access to execution details keyed by execution detail type.
/// </summary>
public interface IExecutionContent
{
    Dictionary<eExecutionDetails, IExecutionDetails> ExecutionDetails { get; set; }
}
