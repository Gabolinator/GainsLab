using GainsLab.Models.Core;
using GainsLab.Models.DataManagement.DB.Model.DTOs;
using GainsLab.Models.Logging;
using GainsLab.Models.WorkoutComponents.MovementCategory;
using Microsoft.EntityFrameworkCore;

namespace GainsLab.Models.DataManagement.DB;

public class GainLabDBContext : DbContext
{
    private readonly ILogger _logger;
    
    public GainLabDBContext(DbContextOptions<GainLabDBContext> options, ILogger logger)
        : base(options)
    {
        _logger = logger;
        _logger.Log(nameof(GainLabDBContext) , "Service intanciated");
    }
    
    public DbSet<EquipmentDTO> Equipments => Set<EquipmentDTO>();
    public DbSet<ComponentDescriptorDTO> Descriptors => Set<ComponentDescriptorDTO>();
    //public DbSet<EquipmentList> EquipmentLists => Set<EquipmentList>();
   // public DbSet<MovementCategory> MovementCategories => Set<MovementCategory>();
 //   public DbSet<MusclesGroup> MuscleGroups => Set<MusclesGroup>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        CreateEquipmentTableModel(modelBuilder);

        
        
       
      
        // modelBuilder.Entity<MovementCategory>(entity =>
        // {
        //     entity.OwnsOne(e => e.Descriptor);
        // });
        //
        // modelBuilder.Entity<EquipmentList>(entity =>
        // {
        //     entity.OwnsOne(e => e.Descriptor);
        // });
        //
        //
        // modelBuilder.Entity<EquipmentList>()
        //     .HasMany(e => e.Equipments)
        //     .WithMany(); 
    }

    private void CreateEquipmentTableModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EquipmentDTO>()
            .HasKey(e => e.Id);  

        modelBuilder.Entity<EquipmentDTO>()
            .Property(e => e.Id)
            .HasColumnType("INTEGER")
            .ValueGeneratedOnAdd(); 

        // modelBuilder.Entity<EquipmentDTO>()
        //     .HasOne(e => e.Descriptor)
        //     .WithMany()
        //     .HasForeignKey(e => e.DescriptorID);
    }
}
    
 
