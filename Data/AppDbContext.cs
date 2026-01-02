using EquipmentAccounting.Models;
using Microsoft.EntityFrameworkCore;

namespace EquipmentAccounting.Data;

public class AppDbContext : DbContext
{
    static AppDbContext()
    {
        // Use timestamp without time zone for DateTime (avoids UTC conversion issues)
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<RightsOwner> RightsOwners { get; set; }
    public DbSet<Film> Films { get; set; }
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<TvScheduleEntry> TvScheduleEntries { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=equipment_db;Username=equipment_user;Password=equipment_pass");
        optionsBuilder.UseLazyLoadingProxies();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User -> Role relationship
        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Film -> RightsOwner relationship
        modelBuilder.Entity<Film>()
            .HasOne(f => f.RightsOwner)
            .WithMany(r => r.Films)
            .HasForeignKey(f => f.RightsOwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        // RightsOwner -> Contact relationship
        modelBuilder.Entity<RightsOwner>()
            .HasOne(r => r.Contact)
            .WithMany(c => c.RightsOwners)
            .HasForeignKey(r => r.ContactId)
            .OnDelete(DeleteBehavior.SetNull);

        // TvScheduleEntry -> Film relationship
        modelBuilder.Entity<TvScheduleEntry>()
            .HasOne(t => t.Film)
            .WithMany(f => f.ScheduleEntries)
            .HasForeignKey(t => t.FilmId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Login)
            .IsUnique();

        modelBuilder.Entity<Role>()
            .HasIndex(r => r.Name)
            .IsUnique();

        modelBuilder.Entity<Film>()
            .HasIndex(f => f.Title);

        modelBuilder.Entity<TvScheduleEntry>()
            .HasIndex(t => t.ScheduledDateTime);
    }
}
