namespace EquipmentAccounting.Models;

/// <summary>
/// Сущность фильма (произведения) в библиотеке телеканала.
/// Содержит информацию о фильме, правах на показ и связь с правообладателем.
/// </summary>
public class Film
{
    /// <summary>Уникальный идентификатор фильма</summary>
    public int Id { get; set; }

    /// <summary>Название фильма</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Возрастное ограничение (например, "12+", "16+")</summary>
    public string AgeRestriction { get; set; } = string.Empty;

    /// <summary>Хронометраж в минутах</summary>
    public int Duration { get; set; }

    /// <summary>Путь к файлу фильма на диске</summary>
    public string FilePath { get; set; } = string.Empty;

    // ===== Информация о правах на показ =====

    /// <summary>Дата закупки прав на показ</summary>
    public DateTime? PurchaseDate { get; set; }

    /// <summary>Дата окончания прав на показ</summary>
    public DateTime? RightsExpirationDate { get; set; }

    /// <summary>Количество оставшихся разрешённых показов</summary>
    public int ShowCount { get; set; }

    /// <summary>Дата добавления записи в систему</summary>
    public DateTime DateAdded { get; set; } = DateTime.Now;

    // ===== Связи =====

    /// <summary>Внешний ключ на правообладателя</summary>
    public int RightsOwnerId { get; set; }

    /// <summary>Навигационное свойство - правообладатель фильма</summary>
    public virtual RightsOwner RightsOwner { get; set; } = null!;

    /// <summary>Навигационное свойство - записи в телепрограмме с этим фильмом</summary>
    public virtual ICollection<TvScheduleEntry> ScheduleEntries { get; set; } = new List<TvScheduleEntry>();

    /// <summary>
    /// Вычисляемое свойство: действительны ли права на показ.
    /// Проверяет, что права не истекли и есть оставшиеся показы.
    /// </summary>
    public bool HasValidRights =>
        (!RightsExpirationDate.HasValue || RightsExpirationDate.Value >= DateTime.Today)
        && ShowCount > 0;
}
