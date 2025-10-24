
using GainsLab.Core.Models.Core.Utilities.Logging;
using GainsLab.Infrastructure.DB.DTOs;
using GainsLab.Models.DataManagement.DB.Model.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GainsLab.Infrastructure.DB.Context;

//db context for postgress
public class GainLabPgDBContext(DbContextOptions< GainLabPgDBContext> options) : DbContext(options)
{
    private ILogger? _logger;
    

    public DbSet<EquipmentDTO> Equipments => Set<EquipmentDTO>();
    public DbSet<DescriptorDTO> Descriptors => Set<DescriptorDTO>();

    public DbSet<UserDto> Users => Set<UserDto>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        _logger?.Log("GainLabPgDBContext", "Model Creating");
        modelBuilder.HasDefaultSchema("public");
    
        CreateEquipmentTableModel(modelBuilder);
       // CreateUserTableModel(modelBuilder);
       
    }

    private void CreateUserTableModel(ModelBuilder modelBuilder)
    {
        _logger?.Log("GainLabPgDBContext", "Creating User Table");
        modelBuilder.Entity<UserDto>()
            .HasKey(e => e.Id);  

        modelBuilder.Entity<UserDto>()
            .Property(e => e.Id)
            .HasColumnType("INTEGER")
            .ValueGeneratedOnAdd(); 
        
        modelBuilder.Entity<UserDto>()
            .Property(x => x.Version)
            .IsConcurrencyToken();
    }

    private void CreateEquipmentTableModel(ModelBuilder modelBuilder)
    {
        
        _logger?.Log("GainLabPgDBContext", "Creating Equipment Table");
        modelBuilder.Entity<EquipmentDTO>(e =>
        {
            e.ToTable("equipments");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired();
            e.HasOne(x => x.Descriptor)
                .WithMany()
                .HasForeignKey(x => x.DescriptorID);
        });
        
    }
        
        // b.Entity<Exercise>().HasKey(x => x.Id);
        // b.Entity<Workout>().HasKey(x => x.Id);
        // b.Entity<Set>().HasKey(x => x.Id);
        // b.Entity<Workout>().HasMany(w => w.Sets).WithOne().HasForeignKey(s => s.WorkoutId);
        // // simple optimistic concurrency
        // b.Entity<Exercise>().Property(x => x.Version).IsConcurrencyToken();
        // b.Entity<Workout>().Property(x => x.Version).IsConcurrencyToken();
        // b.Entity<Set>().Property(x => x.Version).IsConcurrencyToken();

        public void AddLogger(ILogger logger)
        {
            _logger = logger;
        }
}