using System;
using GainsLab.Infrastructure.DB;
using GainsLab.Infrastructure.DB.Context;
using GainsLab.Infrastructure.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GainsLab.Models.DataManagement.DB;

/// <summary>
/// Design-time factory used by EF Core tooling to create the SQLite context.
/// </summary>
public class GainLabDBContextFactory : IDesignTimeDbContextFactory<GainLabSQLDBContext>
{
    /// <inheritdoc />
    public GainLabSQLDBContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<GainLabSQLDBContext>();
        optionsBuilder.UseSqlite("Data Source=gainlab.db");

        // Logger is optional for design-time; provide a dummy
        var dummyLogger = new GainsLabLogger();

        return new GainLabSQLDBContext(optionsBuilder.Options, dummyLogger);
    }
    
}
