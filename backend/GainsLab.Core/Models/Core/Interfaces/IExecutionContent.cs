using System.Collections.Generic;

namespace GainsLab.Models.Core.Interfaces;

public interface IExecutionContent
{
    Dictionary<eExecutionDetails, IExecutionDetails> ExecutionDetails { get; set; }
}