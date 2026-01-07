
using GainsLab.Application.Interfaces.DataManagement.Gateway;
using GainsLab.Application.Interfaces.DataManagement.Provider;
using GainsLab.Application.Interfaces.Sync;

using GainsLab.Domain.Interfaces;
using GainsLab.Infrastructure;
using GainsLab.Infrastructure.Api;
using GainsLab.Infrastructure.Api.Gateway;
using GainsLab.Infrastructure.Api.Interface;
using GainsLab.Infrastructure.Logging;
using GainsLab.Infrastructure.SyncService;
using GainsLab.Infrastructure.Utilities;
using GainsLab.WebLayer.Components;
using ILogger = GainsLab.Domain.Interfaces.ILogger;

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

void AddApis(IServiceCollection s)
{
    s.AddHttpClient<IDescriptorApi, DescriptorApi>(ConfigureSyncClient);
    s.AddHttpClient<IEquipmentApi, EquipmentApi>(ConfigureSyncClient);
    s.AddScoped<IApiClientRegistry, ApiClientRegistry>();
}

void AddProvider(IServiceCollection s)
{
    s.AddScoped<IDescriptorProvider, HttpDataProvider>();
    s.AddScoped<IEquipmentProvider,HttpDataProvider>();
    s.AddScoped<IMuscleProvider,HttpDataProvider>();
}

void AddGateway(IServiceCollection s)
{
    s.AddScoped<IDescriptorGateway, DescriptorGateway>();
    s.AddScoped<IEquipmentGateway,EquipmentGateway>();
    s.AddScoped<IMuscleGateway,MuscleGateway>();
}


AddApis(builder.Services);



builder.Services.AddHttpClient<IRemoteProvider, HttpDataProvider>(ConfigureSyncClient);
builder.Services.AddHttpClient("SyncApi", ConfigureSyncClient);
builder.Services.AddScoped<IEntitySyncClient, EntitySyncClient>();

AddProvider(builder.Services);
AddGateway(builder.Services);

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