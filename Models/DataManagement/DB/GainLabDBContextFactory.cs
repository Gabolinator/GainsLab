using System;
using GainsLab.Models.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GainsLab.Models.DataManagement.DB;

public class GainLabDBContextFactory : IDesignTimeDbContextFactory<GainLabDBContext>
{
    public GainLabDBContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<GainLabDBContext>();
        optionsBuilder.UseSqlite("Data Source=gainlab.db");

        // Logger is optional for design-time; provide a dummy
        var dummyLogger = new WorkoutLogger();

        return new GainLabDBContext(optionsBuilder.Options, dummyLogger);
    }
    
}