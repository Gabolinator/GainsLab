using GainsLab.Domain.Interfaces;
using GainsLab.Infrastructure.DB;
using GainsLab.Infrastructure.DB.Context;
using GainsLab.Infrastructure.Utilities;
using Microsoft.EntityFrameworkCore;
using ILogger = GainsLab.Domain.Interfaces.ILogger;


namespace GainsLab.Api.Extensions;

public static class AppExtension
{
    
    public static void ConfigureRequestPipeline(this WebApplication app)
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


    public static void MapEndpoints(this WebApplication app)
    {
        app.MapControllers();
        app.MapGet("/healthz", HandleHealthz);
    }

    public static async Task RunApplicationAsync(this WebApplication app, ILogger logger, Clock clock)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GainLabPgDBContext>();
        var resolver = scope.ServiceProvider.GetRequiredService<IEntitySeedResolver>();
    
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

        //todo remove me after testing , we will seed db from the data management console.
        logger.Log("Initializing DB");
       // await dbInitializer.CreateBaseEntities(db,resolver);
        logger.Log("Initialize DB - completed");

        await app.RunAsync();
    }

    static IResult HandleHealthz() => Results.Ok(new { ok = true });
    
}