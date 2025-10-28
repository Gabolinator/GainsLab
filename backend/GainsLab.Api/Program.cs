

using GainsLab.Contracts.Interface;
using GainsLab.Contracts.SyncDto;
using GainsLab.Contracts.SyncService;
using GainsLab.Core.Models.Core.Utilities;
using GainsLab.Core.Models.Core.Utilities.Logging;
using GainsLab.Infrastructure.DB;
using GainsLab.Infrastructure.DB.Context;
using Microsoft.EntityFrameworkCore;
using Results = Microsoft.AspNetCore.Http.Results;


var logger = new GainsLabLogger("BACKEND");
var clock = new Clock();

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
var services = builder.Services;


//todo remove fro prod
builder.Environment.EnvironmentName = Environments.Development;
config.AddUserSecrets<Program>(optional: true);


logger.Log($"ENV: {builder.Environment.EnvironmentName}");
logger.Log($"Conn (pre): '{builder.Configuration.GetConnectionString("GainsLabDb") ?? "<null>"}'");


services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

var cs = builder.Configuration.GetConnectionString("GainsLabDb");
if (string.IsNullOrWhiteSpace(cs))
    throw new InvalidOperationException("Missing ConnectionStrings:GainsLabDb. Add it to appsettings.Development.json or user-secrets.");



services.AddDbContext<GainLabPgDBContext>(o =>
    o.UseNpgsql(cs, npgsql => npgsql.EnableRetryOnFailure()));

services.AddSingleton<GainsLab.Core.Models.Core.Utilities.Logging.ILogger>(logger);

services.AddScoped<ISyncService<EquipmentSyncDto>, EquipmentSyncService>();
services.AddScoped<ISyncService<DescriptorSyncDto>, DescriptorSyncService>();

// Also expose as non-generic so the controller can enumerate:
services.AddScoped<ISyncService>(sp => sp.GetRequiredService<ISyncService<EquipmentSyncDto>>());
services.AddScoped<ISyncService>(sp => sp.GetRequiredService<ISyncService<DescriptorSyncDto>>());


// Optional CORS for your client app
// services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
// app.UseAuthentication();
app.UseAuthorization();
// app.UseCors();

app.MapControllers();
app.MapGet("/healthz", () => Results.Ok(new { ok = true }));


using (var scope = app.Services.CreateScope())
{
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