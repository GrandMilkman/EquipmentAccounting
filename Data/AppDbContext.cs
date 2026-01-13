using EquipmentAccounting.Models;
using Microsoft.EntityFrameworkCore;

namespace EquipmentAccounting.Data;

/// <summary>
/// Контекст базы данных Entity Framework Core.
/// Управляет подключением к PostgreSQL и определяет модель данных.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Статический конструктор. Настраивает поведение timestamp для PostgreSQL.
    /// </summary>
    static AppDbContext()
    {
        // Использование timestamp без часового пояса (избегает проблем с конвертацией UTC)
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    /// <summary>Таблица пользователей системы</summary>
    public DbSet<User> Users { get; set; }

    /// <summary>Таблица ролей с правами доступа</summary>
    public DbSet<Role> Roles { get; set; }

    /// <summary>Таблица правообладателей контента</summary>
    public DbSet<RightsOwner> RightsOwners { get; set; }

    /// <summary>Таблица фильмов</summary>
    public DbSet<Film> Films { get; set; }

    /// <summary>Таблица контактов продавцов прав</summary>
    public DbSet<Contact> Contacts { get; set; }

    /// <summary>Таблица записей телепрограммы</summary>
    public DbSet<TvScheduleEntry> TvScheduleEntries { get; set; }

    /// <summary>
    /// Конфигурация подключения к базе данных.
    /// Использует PostgreSQL с lazy loading прокси.
    /// </summary>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Строка подключения к PostgreSQL (локальный Docker-контейнер)
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=equipment_db;Username=equipment_user;Password=equipment_pass");
        // Включение lazy loading для навигационных свойств
        optionsBuilder.UseLazyLoadingProxies();
    }

    /// <summary>
    /// Конфигурация модели данных: связи между сущностями, индексы, каскадное удаление.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User -> Role: многие к одному, запрет удаления роли с пользователями
        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Film -> RightsOwner: многие к одному, каскадное удаление фильмов при удалении правообладателя
        modelBuilder.Entity<Film>()
            .HasOne(f => f.RightsOwner)
            .WithMany(r => r.Films)
            .HasForeignKey(f => f.RightsOwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        // RightsOwner -> Contact: многие к одному, обнуление FK при удалении контакта
        modelBuilder.Entity<RightsOwner>()
            .HasOne(r => r.Contact)
            .WithMany(c => c.RightsOwners)
            .HasForeignKey(r => r.ContactId)
            .OnDelete(DeleteBehavior.SetNull);

        // TvScheduleEntry -> Film: многие к одному, каскадное удаление записей при удалении фильма
        modelBuilder.Entity<TvScheduleEntry>()
            .HasOne(t => t.Film)
            .WithMany(f => f.ScheduleEntries)
            .HasForeignKey(t => t.FilmId)
            .OnDelete(DeleteBehavior.Cascade);

        // Индексы для оптимизации поиска

        // Уникальный индекс на логин пользователя
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Login)
            .IsUnique();

        // Уникальный индекс на название роли
        modelBuilder.Entity<Role>()
            .HasIndex(r => r.Name)
            .IsUnique();

        // Индекс на название фильма для быстрого поиска
        modelBuilder.Entity<Film>()
            .HasIndex(f => f.Title);

        // Индекс на дату показа для быстрой фильтрации программы
        modelBuilder.Entity<TvScheduleEntry>()
            .HasIndex(t => t.ScheduledDateTime);
    }
}
