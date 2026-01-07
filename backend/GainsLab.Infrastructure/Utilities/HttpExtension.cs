namespace GainsLab.Infrastructure.Utilities;

public static class HttpExtension
{
    public static string DescribeBaseAddress(this HttpClient http) => http.BaseAddress?.ToString().TrimEnd('/') ?? "an unknown base address";

}