using GainsLab.Domain.Interfaces;

namespace GainsLab.Application.Interfaces;

public interface INetworkChecker
{
    public Task<bool> HasInternetAsync(ILogger? logger, int timeoutMs = 3000);
}