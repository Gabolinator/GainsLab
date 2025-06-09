using GainsLab.Models.Core;
using GainsLab.Models.Core.WorkoutComponents;
using GainsLab.Models.Logging;
using GainsLab.Models.WorkoutComponents.MovementCategory;
using Microsoft.EntityFrameworkCore;

namespace GainsLab.Models.DataManagement.DB;

public class GainLabDBContext : DbContext
{
    private readonly IWorkoutLogger _logger;
    
    public GainLabDBContext(DbContextOptions<GainLabDBContext> options, IWorkoutLogger logger)
        : base(options)
    {
        _logger = logger;
        _logger.Log(nameof(GainLabDBContext) , "Service intanciated");
    }
    
    public DbSet<Equipment> Equipments => Set<Equipment>();
    public DbSet<EquipmentList> EquipmentLists => Set<EquipmentList>();
    public DbSet<MovementCategory> MovementCategories => Set<MovementCategory>();
 //   public DbSet<MusclesGroup> MuscleGroups => Set<MusclesGroup>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        
        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.OwnsOne(e => e.Descriptor);
        });
        
        modelBuilder.Entity<MovementCategory>(entity =>
        {
            entity.OwnsOne(e => e.Descriptor);
        });
        
        modelBuilder.Entity<EquipmentList>(entity =>
        {
            entity.OwnsOne(e => e.Descriptor);
        });
        
        
        modelBuilder.Entity<EquipmentList>()
            .HasMany(e => e.Equipments)
            .WithMany(); 
    }
}
    
 
