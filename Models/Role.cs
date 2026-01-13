namespace EquipmentAccounting.Models;

/// <summary>
/// Сущность роли пользователя с гранулярными правами доступа.
/// Определяет, какие действия может выполнять пользователь в системе.
/// </summary>
public class Role
{
    /// <summary>Уникальный идентификатор роли</summary>
    public int Id { get; set; }

    /// <summary>Название роли (уникальное)</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Описание назначения роли</summary>
    public string Description { get; set; } = string.Empty;

    // ===== Права доступа =====

    /// <summary>Управление пользователями (создание, редактирование, удаление)</summary>
    public bool CanManageUsers { get; set; }

    /// <summary>Управление ролями и их правами</summary>
    public bool CanManageRoles { get; set; }

    /// <summary>Создание новых правообладателей</summary>
    public bool CanCreateRightsOwners { get; set; }

    /// <summary>Редактирование правообладателей</summary>
    public bool CanEditRightsOwners { get; set; }

    /// <summary>Удаление правообладателей</summary>
    public bool CanDeleteRightsOwners { get; set; }

    /// <summary>Создание новых фильмов</summary>
    public bool CanCreateFilms { get; set; }

    /// <summary>Редактирование базовой информации о фильме (название, возраст, хронометраж, путь)</summary>
    public bool CanEditFilmBasicInfo { get; set; }

    /// <summary>Редактирование информации о правах (дата закупки, окончание прав, количество показов)</summary>
    public bool CanEditFilmRightsInfo { get; set; }

    /// <summary>Удаление фильмов</summary>
    public bool CanDeleteFilms { get; set; }

    /// <summary>Управление контактами продавцов прав</summary>
    public bool CanManageContacts { get; set; }

    /// <summary>Управление телепрограммой (добавление, редактирование, удаление записей)</summary>
    public bool CanManageSchedule { get; set; }

    /// <summary>Просмотр контента (правообладатели и фильмы)</summary>
    public bool CanViewContent { get; set; }

    /// <summary>Просмотр телепрограммы</summary>
    public bool CanViewSchedule { get; set; }

    /// <summary>Просмотр контактов</summary>
    public bool CanViewContacts { get; set; }

    /// <summary>Навигационное свойство - пользователи с этой ролью</summary>
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
