using GainsLab.Domain.Interfaces;

namespace GainsLab.Domain.Utilities;

public class GuidGenerator : IGuidGenerator
{
    public Guid New() => Guid.NewGuid();
}