


using GainsLab.Domain.Interfaces;

namespace GainsLab.Models.Utilities;

using System;
using System.Net.Http;
using System.Threading.Tasks;

public static class NetworkChecker
{
    public static async Task<bool> HasInternetAsync(ILogger? logger, int timeoutMs = 3000)
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