using GainsLab.Domain.Interfaces;

namespace GainsLab.Infrastructure.Utilities;

/// <summary>
/// Default implementation of <see cref="IClock"/> that reads from the system clock.
/// </summary>
public class Clock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
