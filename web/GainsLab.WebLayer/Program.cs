
using GainsLab.Contracts;
using GainsLab.Contracts.Interface;
using GainsLab.Contracts.SyncService;
using GainsLab.Core.Models.Core.Interfaces;
using GainsLab.Core.Models.Core.Interfaces.DataManagement;
using GainsLab.Core.Models.Core.Utilities;
using GainsLab.Core.Models.Core.Utilities.Logging;
using GainsLab.WebLayer.Components;
using ILogger = GainsLab.Core.Models.Core.Utilities.Logging.ILogger;

var builder = WebApplication.CreateBuilder(args);

var webLogger = new GainsLabLogger("WebLayer");
webLogger.ToggleDecoration(true);



builder.Services.AddSingleton<ILogger>(webLogger);
builder.Services.AddSingleton<IClock,Clock>();

Uri ResolveSyncBaseAddress(ILogger? logger)
{
    const string defaultBase = "https://localhost:5001/";
    var configured = Environment.GetEnvironmentVariable("GAINS_SYNC_BASE_URL");
    var candidate = string.IsNullOrWhiteSpace(configured) ? defaultBase : configured.Trim();

    if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri) ||
        (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
    {
        var message =
            $"GAINS_SYNC_BASE_URL must be an absolute http/https URL. Current value: '{candidate}'.";
        logger?.LogError("Blazor Main", message);
        throw new InvalidOperationException(message);
    }

    var normalized = uri.ToString();
    if (!normalized.EndsWith("/")) normalized += "/";

    logger?.Log("Blazor Main", $"Using sync server base address {normalized}");
    return new Uri(normalized);
}

void ConfigureSyncClient(IServiceProvider sp, HttpClient client)
{
 
    var baseAddress = ResolveSyncBaseAddress(webLogger);
    client.BaseAddress = baseAddress;
    client.Timeout = TimeSpan.FromSeconds(30);
}

builder.Services.AddHttpClient<IRemoteProvider, HttpDataProvider>(ConfigureSyncClient);
builder.Services.AddHttpClient("SyncApi", ConfigureSyncClient);
builder.Services.AddSingleton<IEntitySyncClient, EntitySyncClient>();

// Add services to the container.
builder.Services.AddRazorComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>();

app.Run();