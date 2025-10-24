using GainsLab.Core.Models.Core.Utilities.Logging;
using GainsLab.Infrastructure.DB.DTOs;
using GainsLab.Models.DataManagement.DB.Model.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GainsLab.Infrastructure.DB.Context;

public class GainLabSQLDBContext : DbContext
{
    private readonly ILogger _logger;
    
    public GainLabSQLDBContext(DbContextOptions<GainLabSQLDBContext> options, ILogger logger)
        : base(options)
    {
        _logger = logger;
        _logger.Log(nameof(GainLabSQLDBContext) , "Service intanciated");
    }
    
    public DbSet<EquipmentDTO> Equipments => Set<EquipmentDTO>();
    public DbSet<DescriptorDTO> Descriptors => Set<DescriptorDTO>();

    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        CreateEquipmentTableModel(modelBuilder);
        
    }

    private void CreateEquipmentTableModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EquipmentDTO>()
            .HasKey(e => e.Id);  

        modelBuilder.Entity<EquipmentDTO>()
            .Property(e => e.Id)
            .HasColumnType("INTEGER")
            .ValueGeneratedOnAdd(); 
        
    }
}
    
 
