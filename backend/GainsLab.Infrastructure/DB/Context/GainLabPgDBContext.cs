using GainsLab.Infrastructure.DB.DTOs;
using GainsLab.Models.DataManagement.DB.Model.DTOs;
using GainsLab.Models.Logging;
using Microsoft.EntityFrameworkCore;

namespace GainsLab.Infrastructure.DB.Context;

//db context for postgress
public class GainLabPgDBContext(DbContextOptions< GainLabPgDBContext> options) : DbContext(options)
{
  //  public DbSet<Exercise> Exercises => Set<Exercise>();
   // public DbSet<Workout>  Workouts  => Set<Workout>();
    //public DbSet<Set>      Sets      => Set<Set>();
    public DbSet<EquipmentDTO> Equipments => Set<EquipmentDTO>();
    public DbSet<DescriptorDTO> Descriptors => Set<DescriptorDTO>();
    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
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
        
        modelBuilder.Entity<EquipmentDTO>()
            .Property(x => x.Version)
            .IsConcurrencyToken();
        
    }
        
        // b.Entity<Exercise>().HasKey(x => x.Id);
        // b.Entity<Workout>().HasKey(x => x.Id);
        // b.Entity<Set>().HasKey(x => x.Id);
        // b.Entity<Workout>().HasMany(w => w.Sets).WithOne().HasForeignKey(s => s.WorkoutId);
        // // simple optimistic concurrency
        // b.Entity<Exercise>().Property(x => x.Version).IsConcurrencyToken();
        // b.Entity<Workout>().Property(x => x.Version).IsConcurrencyToken();
        // b.Entity<Set>().Property(x => x.Version).IsConcurrencyToken();
    
}