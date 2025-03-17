using EquipmentAccounting.Models;
using Microsoft.EntityFrameworkCore;

namespace EquipmentAccounting.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }

    public DbSet<Belarusfilm> Belarusfilms { get; set; }
    public DbSet<Volga> Volgas { get; set; }
    public DbSet<FPL> FPLs { get; set; }
    public DbSet<Paramount> Paramounts { get; set; }
    public DbSet<WarnerBros> WarnerBrosSet { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // БД сохраняется в папке bin\Debug\net9.0-windows
        optionsBuilder.UseSqlite("Data Source=films.db");
    }
}