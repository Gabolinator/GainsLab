using GainsLab.Models.Core;
using GainsLab.Models.Core.Interfaces;

namespace GainsLab.Core.Models.Core.Interfaces;

/// <summary>
/// Provides access to execution details keyed by execution detail type.
/// </summary>
public interface IExecutionContent
{
    Dictionary<eExecutionDetails, IExecutionDetails> ExecutionDetails { get; set; }
}
