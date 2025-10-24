using GainsLab.Models.Core;

namespace GainsLab.Core.Models.Core.Interfaces;

/// <summary>
/// Base class for execution detail payloads tagged with a detail type.
/// </summary>
public class IExecutionDetails
{
    private eExecutionDetails DetailType { get; set; }
}
