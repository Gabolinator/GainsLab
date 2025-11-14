using GainsLab.Contracts.Interface;
using GainsLab.Contracts.SyncDto;
using GainsLab.Contracts.SyncService;
using GainsLab.Core.Models.Core.Utilities;
using GainsLab.Core.Models.Core.Utilities.Logging;
using GainsLab.Infrastructure.DB;
using GainsLab.Infrastructure.DB.Context;
using Microsoft.EntityFrameworkCore;
using ILogger = GainsLab.Core.Models.Core.Utilities.Logging.ILogger;
using Results = Microsoft.AspNetCore.Http.Results;

var logger = new GainsLabLogger("BACKEND");
logger.ToggleDecoration(false);
var clock = new Clock();

var builder = WebApplication.CreateBuilder(args);
ConfigureEnvironment(builder, logger);
ConfigureServices(builder.Services, builder.Configuration, logger);

var app = builder.Build();
ConfigureRequestPipeline(app);
MapEndpoints(app);

await RunApplicationAsync(app, logger, clock);


static void ConfigureEnvironment(WebApplicationBuilder builder, ILogger logger)
{
    // TODO: remove or make configurable before shipping to production.
    builder.Environment.EnvironmentName = Environments.Development;
    builder.Configuration.AddUserSecrets<Program>(optional: true);

    logger.Log($"ENV: {builder.Environment.EnvironmentName}");
    logger.Log($"Conn (pre): '{builder.Configuration.GetConnectionString("GainsLabDb") ?? "<null>"}'");
}


static void ConfigureServices(IServiceCollection services, IConfiguration configuration, ILogger logger)
{
    services.AddControllers();
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();

    var connectionString = configuration.GetConnectionString("GainsLabDb");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "Missing ConnectionStrings:GainsLabDb. Add it to appsettings.Development.json or user-secrets.");
    }

    services.AddDbContext<GainLabPgDBContext>(options =>
        options.UseNpgsql(connectionString, npgsql => npgsql.EnableRetryOnFailure()));

    services.AddSingleton<ILogger>(logger);

    services.AddScoped<ISyncService<EquipmentSyncDTO>, EquipmentSyncService>();
    services.AddScoped<ISyncService<DescriptorSyncDTO>, DescriptorSyncService>();
    services.AddScoped<ISyncService<MuscleSyncDTO>, MuscleSyncService>();

    
    // Also expose as non-generic so the controller can enumerate:
    services.AddScoped<ISyncService>(sp => sp.GetRequiredService<ISyncService<EquipmentSyncDTO>>());
    services.AddScoped<ISyncService>(sp => sp.GetRequiredService<ISyncService<DescriptorSyncDTO>>());
    services.AddScoped<ISyncService>(sp => sp.GetRequiredService<ISyncService<MuscleSyncDTO>>());

    // Optional CORS for your client app
    // services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
}


static void ConfigureRequestPipeline(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    // app.UseAuthentication();
    app.UseAuthorization();
    // app.UseCors();
}


static void MapEndpoints(WebApplication app)
{
    app.MapControllers();
    app.MapGet("/healthz", HandleHealthz);
}


static async Task RunApplicationAsync(WebApplication app, ILogger logger, Clock clock)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<GainLabPgDBContext>();

    db.AddLogger(logger);
    db.AddClock(clock);

    logger.Log("Migrating DB");
    await db.Database.MigrateAsync();
    logger.Log("Migrating DB - Completed");

    var pending = (await db.Database.GetPendingMigrationsAsync()).ToArray();
    logger.Log("Pending migrations: " + (pending.Length == 0 ? "none" : string.Join(", ", pending)));

    var applied = (await db.Database.GetAppliedMigrationsAsync()).ToArray();
    logger.Log("Applied migrations: " + (applied.Length == 0 ? "none" : string.Join(", ", applied)));

    logger.Log("Can connect: " + await db.Database.CanConnectAsync());
    logger.Log("Database provider: " + db.Database.ProviderName);
    logger.Log("Connection string hash (for sanity): " + db.Database.GetDbConnection().ConnectionString.GetHashCode());

    var dbInitializer = new DBDataInitializer(logger, clock);

    logger.Log("Initializing DB");
    await dbInitializer.CreateBaseEntities(db);
    logger.Log("Initialize DB - completed");

    await app.RunAsync();
}

static IResult HandleHealthz() => Results.Ok(new { ok = true });

/// <summary>
/// Partial Program class required for user secrets configuration.
/// </summary>
public partial class Program { }
