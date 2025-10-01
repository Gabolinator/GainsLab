using GainsLab.Models.Core.Interfaces;

namespace GainsLab.Models.Utilities;

public class Clock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}