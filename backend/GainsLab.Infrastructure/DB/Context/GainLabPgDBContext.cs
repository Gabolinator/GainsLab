
using GainsLab.Core.Models.Core.Interfaces;
using GainsLab.Core.Models.Core.Utilities.Logging;
using GainsLab.Infrastructure.DB.DTOs;
using GainsLab.Models.DataManagement.DB.Model.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GainsLab.Infrastructure.DB.Context;

//db context for postgress
public class GainLabPgDBContext(DbContextOptions< GainLabPgDBContext> options) : DbContext(options)
{
    private ILogger? _logger;
    private IClock _clock;


    public DbSet<EquipmentDTO> Equipments => Set<EquipmentDTO>();
    public DbSet<DescriptorDTO> Descriptors => Set<DescriptorDTO>();

    public DbSet<UserDto> Users => Set<UserDto>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        _logger?.Log("GainLabPgDBContext", "Model Creating");
        modelBuilder.HasDefaultSchema("public");
        CreateDescriptorTableModel(modelBuilder);
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

            e.HasIndex(x => new { x.UpdatedAtUtc, x.UpdatedSeq });;
        });
        
        
        
    }

    private void CreateDescriptorTableModel(ModelBuilder modelBuilder)
    {

        _logger?.Log("GainLabPgDBContext", "Creating Descriptor Table");
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

            foreach (var entry in ChangeTracker.Entries<BaseDto>())
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