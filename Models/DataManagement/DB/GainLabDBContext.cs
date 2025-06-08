using Microsoft.EntityFrameworkCore;

namespace GainsLab.Models.DataManagement.DB;

public class GainLabDBContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Creates a SQLite file in your app's folder
       optionsBuilder.UseSqlite("Data Source=gainlab.db");
    }
}