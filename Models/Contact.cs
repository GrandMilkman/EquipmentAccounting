namespace EquipmentAccounting.Models;

/// <summary>
/// Сущность контакта - информация о продавце/дистрибьюторе прав.
/// Хранит контактные данные компании для связи по вопросам закупки прав.
/// </summary>
public class Contact
{
    /// <summary>Уникальный идентификатор контакта</summary>
    public int Id { get; set; }

    /// <summary>Название компании-продавца</summary>
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>Телефон для связи</summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>Электронная почта</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Почтовый адрес</summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>ФИО контактного лица</summary>
    public string ContactPerson { get; set; } = string.Empty;

    /// <summary>Дополнительные примечания</summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>Навигационное свойство - правообладатели, связанные с этим контактом</summary>
    public virtual ICollection<RightsOwner> RightsOwners { get; set; } = new List<RightsOwner>();
}
