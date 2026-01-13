namespace EquipmentAccounting.Models;

/// <summary>
/// Сущность правообладателя (студии, дистрибьютора).
/// Владелец прав на показ фильмов.
/// </summary>
public class RightsOwner
{
    /// <summary>Уникальный идентификатор правообладателя</summary>
    public int Id { get; set; }

    /// <summary>Название правообладателя (студии, компании)</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Описание правообладателя</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Дата добавления записи в систему</summary>
    public DateTime DateAdded { get; set; } = DateTime.Now;

    // ===== Связи =====

    /// <summary>Внешний ключ на контакт продавца прав (опциональный)</summary>
    public int? ContactId { get; set; }

    /// <summary>Навигационное свойство - контактная информация продавца</summary>
    public virtual Contact? Contact { get; set; }

    /// <summary>Навигационное свойство - фильмы этого правообладателя</summary>
    public virtual ICollection<Film> Films { get; set; } = new List<Film>();
}
