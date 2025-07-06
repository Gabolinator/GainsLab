using System;
using GainsLab.Models.Logging;

namespace GainsLab.Models.Core.Exceptions.DataBaseOperations;

public class DataBaseOperationException : Exception
{
    public DataBaseOperationException( string message, Exception innerException, IWorkoutLogger? logger = null): base(message, innerException)
    {
       if(logger!= null)  logger.LogError("DataBaseOperationException", $"{message} | Inner: {innerException.Message}");
    }
}