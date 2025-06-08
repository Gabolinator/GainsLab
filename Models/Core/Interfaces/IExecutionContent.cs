using System.Collections.Generic;

namespace GainsLab.Models.Core;

public interface IExecutionContent
{
    Dictionary<eExecutionDetails, IExecutionDetails> ExecutionDetails { get; set; }
}