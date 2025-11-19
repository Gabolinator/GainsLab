using System.Linq.Expressions;
using GainsLab.Core.Models.Core;
using GainsLab.Core.Models.Core.Utilities.Logging;
using GainsLab.Infrastructure.DB.DTOs;
using GainsLab.Infrastructure.DB.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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

    public DbSet<OutboxChangeDto> OutboxChanges => Set<OutboxChangeDto>();

    /// <summary>
    /// Gets the table that stores equipment records.
    /// </summary>
    public DbSet<EquipmentDTO> Equipments => Set<EquipmentDTO>();

    /// <summary>
    /// Gets the table that stores descriptor records.
    /// </summary>
    public DbSet<DescriptorDTO> Descriptors => Set<DescriptorDTO>();

    public DbSet<MuscleDTO> Muscles => Set<MuscleDTO>();
    public DbSet<MuscleAntagonistDTO> MuscleAntagonists => Set<MuscleAntagonistDTO>();

    public DbSet<MovementCategoryDTO> MovementCategories => Set<MovementCategoryDTO>();
    public DbSet<MovementCategoryRelationDTO> MovementCategoryRelations => Set<MovementCategoryRelationDTO>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        CreateSyncStateTable_Sqlite(modelBuilder);
        CreateOutBoxTable_Sqlite(modelBuilder);
        CreateEquipmentTableModel_Sqlite(modelBuilder);
        CreateDescriptorTableModel_Sqlite(modelBuilder);
        CreateMuscleTableModel_Sqlite(modelBuilder);
        CreateMovementCategoryTableModel_Sqlite(modelBuilder);
    }

     private void CreateMovementCategoryTableModel_Sqlite(ModelBuilder modelBuilder)
    {
       
          _logger?.Log("GainLabPgDBContext", "Creating Movement Category Table");

        modelBuilder.Entity<MovementCategoryDTO>(m =>
        {
            m.ToTable("movement_category");
            m.HasKey(x => x.Id);

            m.Property(x => x.Name).IsRequired();
            
            m.HasOne(x => x.Descriptor)
                .WithMany()
                .HasForeignKey(x => x.DescriptorID)
                .OnDelete(DeleteBehavior.Restrict);

            m.HasOne(x => x.ParentCategory)
                .WithMany()
                .HasForeignKey(x => x.ParentCategoryDbId)
                .OnDelete(DeleteBehavior.Restrict);
   
            m.HasIndex(x => x.GUID).IsUnique();

            m.Property(x => x.UpdatedAtUtc)
                .IsRequired()
                .HasColumnName("updated_at_utc")
                .HasDefaultValueSql("now()");

            m.Property(x => x.UpdatedSeq)
                .HasColumnName("updated_seq")
                .UseIdentityByDefaultColumn();

            m.Property(x => x.IsDeleted)
                .HasColumnName("is_deleted")
                .HasDefaultValue(false);

            m.Property(x => x.Authority)
                .HasColumnName("authority")
                .HasConversion<int>()
                .HasDefaultValue(DataAuthority.Bidirectional);

            m.HasMany(x => x.BaseCategoryLinks)
                .WithOne(x => x.ChildCategory)
                .HasForeignKey(x => x.ChildCategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            m.HasMany(x => x.ChildCategoryLinks)
                .WithOne(x => x.ParentCategory)
                .HasForeignKey(x => x.ParentCategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            m.HasIndex(x => new { x.UpdatedAtUtc, x.UpdatedSeq });
        });

        modelBuilder.Entity<MovementCategoryRelationDTO>(link =>
        {
            link.ToTable("movement_category_relations");
            link.HasKey(x => new { x.ParentCategoryId, x.ChildCategoryId });

            link.HasOne(x => x.ParentCategory)
                .WithMany(x => x.ChildCategoryLinks)
                .HasForeignKey(x => x.ParentCategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            link.HasOne(x => x.ChildCategory)
                .WithMany(x => x.BaseCategoryLinks)
                .HasForeignKey(x => x.ChildCategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
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
        // Convert DateTimeOffset <-> UTC DateTime (stored as TEXT by EF in SQLite)
        var dtoToUtcDateTime = new ValueConverter<DateTimeOffset, DateTime>(
            (Expression<Func<DateTimeOffset, DateTime>>)(v => v.UtcDateTime),
            (Expression<Func<DateTime, DateTimeOffset>>)(v =>
                new DateTimeOffset(DateTime.SpecifyKind(v, DateTimeKind.Utc))));

        modelBuilder.Entity<OutboxChangeDto>(b =>
        {
            b.ToTable("outbox_changes");
            b.HasKey(x => x.Id);
            b.Property(x => x.Id).ValueGeneratedOnAdd();

            b.Property(x => x.Entity)
                .IsRequired();

            b.Property(x => x.PayloadJson)
                .IsRequired();

            b.Property(x => x.OccurredAt)
                .HasColumnName("occurred_at")
                .HasConversion(dtoToUtcDateTime) // 👈 key line: makes ORDER BY translatable
                .HasColumnType("TEXT")           // ISO-8601 string; lexicographically sortable
                .HasDefaultValueSql("CURRENT_TIMESTAMP"); // SQLite UTC timestamp

            b.Property(x => x.Sent)
                .HasColumnName("sent")
                .HasDefaultValue(false);

            // helpful compound index for dispatch scanning
            b.HasIndex(x => new { x.Sent, x.OccurredAt });
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

            e.Property(x => x.Authority)
                .HasColumnName("authority")
                .HasConversion<int>()
                .HasDefaultValue(DataAuthority.Bidirectional);

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

            d.Property(x => x.Authority)
                .HasColumnName("authority")
                .HasConversion<int>()
                .HasDefaultValue(DataAuthority.Bidirectional);

            d.HasIndex(x => new { x.UpdatedAtUtc, x.UpdatedSeq });
        });
    }

    private void CreateMuscleTableModel_Sqlite(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MuscleDTO>(m =>
        {
            m.ToTable("muscles");
            m.HasKey(x => x.Id);

            m.Property(x => x.Name).IsRequired();

            m.Property(x => x.BodySection)
                .HasColumnName("body_section")
                .HasConversion<int>()
                .HasDefaultValue(eBodySection.undefined);

            m.HasOne(x => x.Descriptor)
                .WithMany()
                .HasForeignKey(x => x.DescriptorID)
                .OnDelete(DeleteBehavior.Restrict);

            m.HasIndex(x => x.GUID).IsUnique();

            m.Property(x => x.UpdatedAtUtc)
                .IsRequired()
                .HasColumnName("updated_at_utc")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            m.Property(x => x.UpdatedSeq)
                .HasColumnName("updated_seq")
                .HasDefaultValue(0);

            m.Property(x => x.IsDeleted)
                .HasColumnName("is_deleted")
                .HasDefaultValue(false);

            m.Property(x => x.Authority)
                .HasColumnName("authority")
                .HasConversion<int>()
                .HasDefaultValue(DataAuthority.Bidirectional);

            m.HasMany(x => x.Antagonists)
                .WithOne(x => x.Muscle)
                .HasForeignKey(x => x.MuscleId)
                .OnDelete(DeleteBehavior.Cascade);

            m.HasMany(x => x.Agonists)
                .WithOne(x => x.Antagonist)
                .HasForeignKey(x => x.AntagonistId)
                .OnDelete(DeleteBehavior.Cascade);

            m.HasIndex(x => new { x.UpdatedAtUtc, x.UpdatedSeq });
        });

        modelBuilder.Entity<MuscleAntagonistDTO>(link =>
        {
            link.ToTable("muscle_antagonists");
            link.HasKey(x => new { x.MuscleId, x.AntagonistId });

            link.HasOne(x => x.Muscle)
                .WithMany(x => x.Antagonists)
                .HasForeignKey(x => x.MuscleId)
                .OnDelete(DeleteBehavior.Cascade);

            link.HasOne(x => x.Antagonist)
                .WithMany(x => x.Agonists)
                .HasForeignKey(x => x.AntagonistId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
