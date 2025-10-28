using GainsLab.Core.Models.Core.Utilities.Logging;
using GainsLab.Infrastructure.DB.DTOs;
using GainsLab.Infrastructure.DB.Outbox;
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
        _logger.Log(nameof(GainLabSQLDBContext), "Service intanciated");
    }

    DbSet<OutboxChangeDto> OutboxChanges => Set<OutboxChangeDto>();
    public DbSet<EquipmentDTO> Equipments => Set<EquipmentDTO>();
    public DbSet<DescriptorDTO> Descriptors => Set<DescriptorDTO>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        CreateOutBoxTable_Sqlite(modelBuilder);
        CreateEquipmentTableModel_Sqlite(modelBuilder);
        CreateDescriptorTableModel_Sqlite(modelBuilder);
    }

    private void CreateOutBoxTable_Sqlite(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxChangeDto>(b =>                                                                                                                        
        {                                                                                                                                                                
            b.ToTable("outbox_changes");                                                                                                                                 
            b.HasKey(x => x.Id);                                                                                                                                         
            b.Property(x => x.Id).ValueGeneratedOnAdd();                                                                                                                 
            b.Property(x => x.Entity).IsRequired();                                                                                                                      
            b.Property(x => x.PayloadJson).IsRequired();                                                                                                                 
            b.Property(x => x.OccurredAt)                                                                                                                                
                .HasColumnName("occurred_at")                                                                                                                            
                .HasDefaultValueSql("CURRENT_TIMESTAMP");                                                                                                                
            b.Property(x => x.Sent)                                                                                                                                      
                .HasColumnName("sent")                                                                                                                                   
                .HasDefaultValue(false);                                                                                                                                 
            b.HasIndex(x => x.Sent);                                                                                                                                     
        });                                                
    }

    private void CreateEquipmentTableModel_Sqlite(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EquipmentDTO>(e =>
        {
            e.ToTable("equipments");
            e.HasKey(x => x.Id);

            e.Property(x => x.Name).IsRequired();

            e.HasOne(x => x.Descriptor)
                .WithMany()
                .HasForeignKey(x => x.DescriptorID)
                .OnDelete(DeleteBehavior.Restrict);

            e.Property(x => x.UpdatedAtUtc)
                .IsRequired()
                .HasColumnName("updated_at_utc")
                .HasDefaultValueSql("CURRENT_TIMESTAMP"); // SQLite

            
            e.Property(x => x.UpdatedSeq)
                .HasColumnName("updated_seq")
                .HasDefaultValue(0); // then set in code if you need monotonic values

            e.Property(x => x.IsDeleted)
                .HasColumnName("is_deleted")
                .HasDefaultValue(false);

            e.HasIndex(x => new { x.UpdatedAtUtc, x.UpdatedSeq });
        });
    }

    private void CreateDescriptorTableModel_Sqlite(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DescriptorDTO>(d =>
        {
            d.ToTable("descriptors");
            d.HasKey(x => x.Id);

            d.Property(x => x.Content).IsRequired();

            d.Property(x => x.UpdatedAtUtc)
                .IsRequired()
                .HasColumnName("updated_at_utc")
                .HasDefaultValueSql("CURRENT_TIMESTAMP"); // SQLite

            d.Property(x => x.UpdatedSeq)
                .HasColumnName("updated_seq")
                .HasDefaultValue(0); // or manage via code/trigger

            d.Property(x => x.IsDeleted)
                .HasColumnName("is_deleted")
                .HasDefaultValue(false);
            

            d.HasIndex(x => new { x.UpdatedAtUtc, x.UpdatedSeq });
        });
    }
}