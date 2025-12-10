using GainsLab.Domain.Interfaces;

namespace GainsLab.Infrastructure.DB.Exceptions.DataBaseOperations;

public class DataBaseOperationException : Exception
{
    public DataBaseOperationException( string message, Exception innerException, ILogger? logger = null): base(message, innerException)
    {
       if(logger!= null)  logger.LogError("DataBaseOperationException", $"{message} | Inner: {innerException.Message}");
    }
}