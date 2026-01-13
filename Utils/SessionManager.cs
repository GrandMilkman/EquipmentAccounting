using EquipmentAccounting.Models;

namespace EquipmentAccounting.Utils;

/// <summary>
/// Статический менеджер сессии пользователя.
/// Хранит информацию о текущем авторизованном пользователе и предоставляет
/// удобный доступ к проверке прав доступа.
/// </summary>
public static class SessionManager
{
    /// <summary>Текущий авторизованный пользователь</summary>
    public static User? CurrentUser { get; private set; }

    /// <summary>Роль текущего пользователя (из навигационного свойства)</summary>
    public static Role? CurrentRole => CurrentUser?.Role;

    /// <summary>
    /// Установка текущего пользователя при успешной авторизации.
    /// </summary>
    /// <param name="user">Авторизованный пользователь с загруженной ролью</param>
    public static void SetCurrentUser(User user)
    {
        CurrentUser = user;
    }

    /// <summary>
    /// Очистка сессии при выходе из системы.
    /// </summary>
    public static void ClearSession()
    {
        CurrentUser = null;
    }

    // ===== Проверки прав доступа =====

    /// <summary>Право на управление пользователями</summary>
    public static bool CanManageUsers => CurrentRole?.CanManageUsers ?? false;

    /// <summary>Право на управление ролями</summary>
    public static bool CanManageRoles => CurrentRole?.CanManageRoles ?? false;

    /// <summary>Право на создание правообладателей</summary>
    public static bool CanCreateRightsOwners => CurrentRole?.CanCreateRightsOwners ?? false;

    /// <summary>Право на редактирование правообладателей</summary>
    public static bool CanEditRightsOwners => CurrentRole?.CanEditRightsOwners ?? false;

    /// <summary>Право на удаление правообладателей</summary>
    public static bool CanDeleteRightsOwners => CurrentRole?.CanDeleteRightsOwners ?? false;

    /// <summary>Право на создание фильмов</summary>
    public static bool CanCreateFilms => CurrentRole?.CanCreateFilms ?? false;

    /// <summary>Право на редактирование базовой информации о фильме</summary>
    public static bool CanEditFilmBasicInfo => CurrentRole?.CanEditFilmBasicInfo ?? false;

    /// <summary>Право на редактирование информации о правах на фильм</summary>
    public static bool CanEditFilmRightsInfo => CurrentRole?.CanEditFilmRightsInfo ?? false;

    /// <summary>Право на удаление фильмов</summary>
    public static bool CanDeleteFilms => CurrentRole?.CanDeleteFilms ?? false;

    /// <summary>Право на управление контактами</summary>
    public static bool CanManageContacts => CurrentRole?.CanManageContacts ?? false;

    /// <summary>Право на управление телепрограммой</summary>
    public static bool CanManageSchedule => CurrentRole?.CanManageSchedule ?? false;

    /// <summary>Право на просмотр контента (правообладатели, фильмы)</summary>
    public static bool CanViewContent => CurrentRole?.CanViewContent ?? false;

    /// <summary>Право на просмотр телепрограммы</summary>
    public static bool CanViewSchedule => CurrentRole?.CanViewSchedule ?? false;

    /// <summary>Право на просмотр контактов</summary>
    public static bool CanViewContacts => CurrentRole?.CanViewContacts ?? false;

    // ===== Комбинированные проверки =====

    /// <summary>Право на редактирование любой информации о фильме</summary>
    public static bool CanEditAnyFilmInfo => CanEditFilmBasicInfo || CanEditFilmRightsInfo;

    /// <summary>Наличие административного доступа (управление пользователями или ролями)</summary>
    public static bool HasAdminAccess => CanManageUsers || CanManageRoles;
}
