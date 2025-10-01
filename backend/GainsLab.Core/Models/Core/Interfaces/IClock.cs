namespace GainsLab.Models.Core.Interfaces;

public interface IClock
{
    DateTimeOffset UtcNow { get;}
}