using System;
using GainsLab.Core.Models.Logging;
using GainsLab.Infrastructure.DB;
using GainsLab.Models.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GainsLab.Models.DataManagement.DB;

public class GainLabDBContextFactory : IDesignTimeDbContextFactory<GainLabSQLDBContext>
{
    public GainLabSQLDBContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<GainLabSQLDBContext>();
        optionsBuilder.UseSqlite("Data Source=gainlab.db");

        // Logger is optional for design-time; provide a dummy
        var dummyLogger = new WorkoutLogger();

        return new GainLabSQLDBContext(optionsBuilder.Options, dummyLogger);
    }
    
}