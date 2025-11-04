using GainsLab.Core.Models.Core.Utilities.Logging;
using GainsLab.Infrastructure.DB.DTOs;
using GainsLab.Infrastructure.DB.Outbox;
using GainsLab.Models.DataManagement.DB.Model.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GainsLab.Infrastructure.DB.Context;

/// <summary>
/// SQLite-backed DbContext used by the desktop application for local persistence.
/// </summary>
public class GainLabSQLDBContext : DbContext
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GainLabSQLDBContext"/> class.
    /// </summary>
    /// <param name="options">Entity Framework options configured for SQLite.</param>
    /// <param name="logger">Logger used to emit diagnostic events.</param>
    public GainLabSQLDBContext(DbContextOptions<GainLabSQLDBContext> options, ILogger logger)
        : base(options)
    {
        _logger = logger;
        _logger.Log(nameof(GainLabSQLDBContext), "Service intanciated");
    }

    /// <summary>
    /// Gets the table that stores global synchronization state.
    /// </summary>
    public DbSet<SyncState> SyncStates => Set<SyncState>();

    DbSet<OutboxChangeDto> OutboxChanges => Set<OutboxChangeDto>();

    /// <summary>
    /// Gets the table that stores equipment records.
    /// </summary>
    public DbSet<EquipmentDTO> Equipments => Set<EquipmentDTO>();

    /// <summary>
    /// Gets the table that stores descriptor records.
    /// </summary>
    public DbSet<DescriptorDTO> Descriptors => Set<DescriptorDTO>();


    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        CreateSyncStateTable_Sqlite(modelBuilder);
        CreateOutBoxTable_Sqlite(modelBuilder);
        CreateEquipmentTableModel_Sqlite(modelBuilder);
        CreateDescriptorTableModel_Sqlite(modelBuilder);
    }

    /// <summary>
    /// Configures the schema for the sync state table when using SQLite.
    /// </summary>
    private void CreateSyncStateTable_Sqlite(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SyncState>(b =>
        {
            b.ToTable("SyncStates");
            b.HasKey(x => x.Partition);
            b.Property(x => x.CursorsJson).HasColumnType("TEXT"); 
        });
    }

    /// <summary>
    /// Configures the schema for the outbox table when using SQLite.
    /// </summary>
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

    /// <summary>
    /// Configures the schema for the equipment table when using SQLite.
    /// </summary>
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

            // GUID unique index
            e.HasIndex(x => x.GUID).IsUnique();

            
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

    /// <summary>
    /// Configures the schema for the descriptor table when using SQLite.
    /// </summary>
    private void CreateDescriptorTableModel_Sqlite(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DescriptorDTO>(d =>
        {
            d.ToTable("descriptors");
            d.HasKey(x => x.Id);

            d.Property(x => x.Content).IsRequired();

            // GUID unique index
            d.HasIndex(x => x.GUID).IsUnique();

            // timestamps & sequence
            d.Property(x => x.UpdatedAtUtc)
                .IsRequired()
                .HasColumnName("updated_at_utc")
                .HasDefaultValueSql("now()");

            d.Property(x => x.UpdatedSeq)
                .HasColumnName("updated_seq")
                .UseIdentityByDefaultColumn();

            d.Property(x => x.IsDeleted)
                .HasColumnName("is_deleted")
                .HasDefaultValue(false);

            d.HasIndex(x => new { x.UpdatedAtUtc, x.UpdatedSeq });
        });
    }
}


