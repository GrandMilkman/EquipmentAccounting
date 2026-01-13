namespace EquipmentAccounting.Models;

/// <summary>
/// Сущность записи в телепрограмме.
/// Представляет запланированный или уже прошедший показ фильма.
/// </summary>
public class TvScheduleEntry
{
    /// <summary>Уникальный идентификатор записи</summary>
    public int Id { get; set; }

    /// <summary>Внешний ключ на фильм для показа</summary>
    public int FilmId { get; set; }

    /// <summary>Навигационное свойство - фильм для показа</summary>
    public virtual Film Film { get; set; } = null!;

    /// <summary>Дата и время запланированного показа</summary>
    public DateTime ScheduledDateTime { get; set; }

    /// <summary>
    /// Флаг: показан ли фильм.
    /// При установке в true счётчик показов фильма уменьшается на 1.
    /// </summary>
    public bool IsAired { get; set; }

    /// <summary>Примечания к записи программы</summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>Дата и время создания записи</summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
