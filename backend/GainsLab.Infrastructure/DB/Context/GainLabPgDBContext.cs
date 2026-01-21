
using GainsLab.Application.DTOs;
using GainsLab.Application.DTOs.Description;
using GainsLab.Application.DTOs.Equipment;
using GainsLab.Application.DTOs.Movement;
using GainsLab.Application.DTOs.MovementCategory;
using GainsLab.Application.DTOs.Muscle;
using GainsLab.Application.DTOs.User;
using GainsLab.Application.Interfaces;
using GainsLab.Domain;
using GainsLab.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GainsLab.Infrastructure.DB.Context;

//db context for postgress
public class GainLabPgDBContext(DbContextOptions< GainLabPgDBContext> options) : DbContext(options) 
{
    private ILogger? _logger;
    private IClock _clock;

    #region DB SETS

    public DbSet<EquipmentRecord> Equipments => Set<EquipmentRecord>();
    public DbSet<DescriptorRecord> Descriptors => Set<DescriptorRecord>();
    
    //muscles
    public DbSet<MuscleRecord> Muscles => Set<MuscleRecord>();
    public DbSet<MuscleAntagonistRecord> MuscleAntagonists => Set<MuscleAntagonistRecord>();

    //categories 
    public DbSet<MovementCategoryRecord> MovementCategories => Set<MovementCategoryRecord>();
    public DbSet<MovementCategoryRelationRecord> MovementCategoryRelations => Set<MovementCategoryRelationRecord>();
    
    
    //movement
    public DbSet<MovementRecord> Movement => Set<MovementRecord>();
    public DbSet<MovementMuscleRelationRecord> MovementMuscleRelations => Set<MovementMuscleRelationRecord>();
    public DbSet<MovementEquipmentRelationRecord> MovementEquipmentRelations => Set<MovementEquipmentRelationRecord>();

    
    //user
    public DbSet<UserRecord> Users => Set<UserRecord>();


    #endregion
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        _logger?.Log("GainLabPgDBContext", "Model Creating");
        modelBuilder.HasDefaultSchema("public");
        CreateDescriptorTableModel(modelBuilder);
        CreateEquipmentTableModel(modelBuilder);
        CreateMuscleTableModel(modelBuilder);
        CreateMovementCategoryTableModel(modelBuilder);
        CreateMovementTableModel(modelBuilder);
        // CreateUserTableModel(modelBuilder);

    }

    private void CreateMovementTableModel(ModelBuilder modelBuilder)
    {
        _logger?.Log("GainLabPgDBContext", "Creating Movement Table");

        //base movement table
        modelBuilder.Entity<MovementRecord>(m =>
        {
            m.ToTable("movement");
            m.HasKey(x => x.Id);
            m.Property(x => x.Name).IsRequired();
            m.HasOne(x=>x.Descriptor) 
                .WithMany()
                .HasForeignKey(x => x.DescriptorID)
                .OnDelete(DeleteBehavior.Restrict);
            
            m.HasOne(x=>x.Category) 
                .WithMany()
                .HasForeignKey(x => x.MovementCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            m.HasIndex(x => x.GUID).IsUnique();
            
                                                                                                                                           
                m.HasOne(m => m.VariantOfMovement)                                                                                                                              
                .WithMany()                                                                                                                                                    
                .HasForeignKey(m => m.VariantOfMovementGuid)                                                                                                                   
                .HasPrincipalKey(m => m.GUID);   
            
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
            
            
            m.HasMany(x => x.EquipmentRelations)
                .WithOne(x => x.Movement)
                .HasForeignKey(x => x.MovementId)
                .OnDelete(DeleteBehavior.Cascade);
            
            m.HasMany(x => x.MuscleRelations)
                .WithOne(x => x.Movement)
                .HasForeignKey(x => x.MovementId)
                .OnDelete(DeleteBehavior.Cascade);
            
            m.HasIndex(x => new { x.UpdatedAtUtc, x.UpdatedSeq });
            
            
        });
        
    
        
        //equipment movement joined table
        modelBuilder.Entity<MovementEquipmentRelationRecord>(link =>
        {
            link.ToTable("movement_equipment_relation");
            link.HasKey(x => new { x.MovementId, x.EquipmentId });
            link.HasOne(x => x.Movement)
                .WithMany(x => x.EquipmentRelations)
                .HasForeignKey(x => x.MovementId)
                .OnDelete(DeleteBehavior.Cascade);

        });
        
        //muscle movement joined table
        modelBuilder.Entity<MovementMuscleRelationRecord>(link =>
        {
            link.ToTable("movement_muscle_relation");
            link.HasKey(x => new { x.MovementId, x.MuscleId });
            link.HasOne(x => x.Movement)
                .WithMany(x => x.MuscleRelations)
                .HasForeignKey(x => x.MovementId)
                .OnDelete(DeleteBehavior.Cascade);

        });

    }

    private void CreateMovementCategoryTableModel(ModelBuilder modelBuilder)
    {
       
          _logger?.Log("GainLabPgDBContext", "Creating Movement Category Table");

        modelBuilder.Entity<MovementCategoryRecord>(m =>
        {
            m.ToTable("movement_category");
            m.HasKey(x => x.Id);
            m.Property(x => x.Id)
                .ValueGeneratedOnAdd();

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

        modelBuilder.Entity<MovementCategoryRelationRecord>(link =>
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

    private void CreateUserTableModel(ModelBuilder modelBuilder)
    {
        _logger?.Log("GainLabPgDBContext", "Creating User Table");
        modelBuilder.Entity<UserRecord>()
            .HasKey(e => e.Id);  

        modelBuilder.Entity<UserRecord>()
            .Property(e => e.Id)
            .HasColumnType("INTEGER")
            .ValueGeneratedOnAdd(); 
        
        modelBuilder.Entity<UserRecord>()
            .Property(x => x.Version)
            .IsConcurrencyToken();
    }

    private void CreateEquipmentTableModel(ModelBuilder modelBuilder)
    {
        
        _logger?.Log("GainLabPgDBContext", "Creating Equipment Table");
        modelBuilder.Entity<EquipmentRecord>(e =>
        {
            e.ToTable("equipments");
            e.HasKey(x => x.Id);

            e.Property(x => x.Name).IsRequired();

            // FK → Descriptor
            e.HasOne(x => x.Descriptor)
                .WithMany()
                .HasForeignKey(x => x.DescriptorID);
                

            // GUID unique index
            e.HasIndex(x => x.GUID).IsUnique();

            // timestamps & sequence
            e.Property(x => x.UpdatedAtUtc)
                .IsRequired()
                .HasColumnName("updated_at_utc")
                .HasDefaultValueSql("now()");

            e.Property(x => x.UpdatedSeq)
                .HasColumnName("updated_seq")
                .UseIdentityByDefaultColumn();

            e.Property(x => x.IsDeleted)
                .HasColumnName("is_deleted")
                .HasDefaultValue(false);

            e.Property(x => x.Authority)
                .HasColumnName("authority")
                .HasConversion<int>()
                .HasDefaultValue(DataAuthority.Bidirectional);

            e.HasIndex(x => new { x.UpdatedAtUtc, x.UpdatedSeq });;
        });
        
        
        
    }

    private void CreateMuscleTableModel(ModelBuilder modelBuilder)
    {
        _logger?.Log("GainLabPgDBContext", "Creating Muscle Table");

        modelBuilder.Entity<MuscleRecord>(m =>
        {
            m.ToTable("muscles");
            m.HasKey(x => x.Id);

            m.Property(x => x.Name).IsRequired();

            m.Property(x => x.BodySection)
                .HasColumnName("body_section")
                .HasConversion<int>()
                .ValueGeneratedNever();

            m.HasOne(x => x.Descriptor)
                .WithMany()
                .HasForeignKey(x => x.DescriptorID)
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

        modelBuilder.Entity<MuscleAntagonistRecord>(link =>
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

    private void CreateDescriptorTableModel(ModelBuilder modelBuilder)
    {

        _logger?.Log("GainLabPgDBContext", "Creating Descriptor Table");
        modelBuilder.Entity<DescriptorRecord>(d =>
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

    public void AddLogger(ILogger logger)
        {
            _logger = logger;
        }

    public void AddClock(IClock clock)
    {
        _clock = clock;
    }

    //todo 
        
        public override Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            var now = _clock?.UtcNow??DateTimeOffset.UtcNow;

            foreach (var entry in ChangeTracker.Entries<BaseRecord>())
            {
                if (entry.State is EntityState.Added or EntityState.Modified)
                {
                    entry.Entity.UpdatedAtUtc = now;
                    // if you track CreatedAtUtc too:
                    if (entry.State == EntityState.Added && entry.Entity.CreatedAtUtc == default)
                        entry.Entity.CreatedAtUtc = now;
                }
            }

            return base.SaveChangesAsync(ct);
        }
        
}
