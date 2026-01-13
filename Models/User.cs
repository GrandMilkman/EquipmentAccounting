namespace EquipmentAccounting.Models;

/// <summary>
/// Сущность пользователя системы.
/// Содержит учётные данные и связь с ролью для определения прав доступа.
/// </summary>
public class User
{
    /// <summary>Уникальный идентификатор пользователя</summary>
    public int Id { get; set; }

    /// <summary>Логин для входа в систему (уникальный)</summary>
    public string Login { get; set; } = string.Empty;

    /// <summary>Пароль пользователя (хранится в открытом виде)</summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>Внешний ключ на роль пользователя</summary>
    public int RoleId { get; set; }

    /// <summary>Навигационное свойство - роль пользователя с правами доступа</summary>
    public virtual Role Role { get; set; } = null!;
}
