using EquipmentAccounting.Models;
using Microsoft.EntityFrameworkCore;

namespace EquipmentAccounting.Data;

public class AppDbContext : DbContext
{
    public DbSet<ActAcceptance> ActAcceptances { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Responsible> Responsibles { get; set; }
    public DbSet<FixedAsset> FixedAssets { get; set; }
    public DbSet<Repair> Repairs { get; set; }
    public DbSet<Transfer> Transfers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Файл базы equipment.db будет при первом запуске в папке приложения по пути bin\Debug\net9.0-windows\equipment.db
        optionsBuilder.UseSqlite("Data Source=equipment.db");
    }
}