using GainsLab.Application.Interfaces;
using GainsLab.Domain.Interfaces;

namespace GainsLab.Infrastructure;

public class NetworkChecker :INetworkChecker
{
    public async Task<bool> HasInternetAsync(ILogger? logger, int timeoutMs = 3000)
    {
        try
        {
            
            using var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMilliseconds(timeoutMs)
            };

           
            using var response = await httpClient.GetAsync("https://www.gstatic.com/generate_204");
            
            logger?.Log(nameof(NetworkChecker), $"Checking Internet connection : {response.IsSuccessStatusCode}");
            
            return response.IsSuccessStatusCode;
        }
        catch
        {
            logger?.LogWarning(nameof(NetworkChecker), $"No Internet connection");
            return false;
        }
    }
}