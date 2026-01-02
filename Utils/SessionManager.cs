using EquipmentAccounting.Models;

namespace EquipmentAccounting.Utils;

public static class SessionManager
{
    public static User? CurrentUser { get; private set; }
    public static Role? CurrentRole => CurrentUser?.Role;

    public static void SetCurrentUser(User user)
    {
        CurrentUser = user;
    }

    public static void ClearSession()
    {
        CurrentUser = null;
    }

    // Permission checks
    public static bool CanManageUsers => CurrentRole?.CanManageUsers ?? false;
    public static bool CanManageRoles => CurrentRole?.CanManageRoles ?? false;
    public static bool CanCreateRightsOwners => CurrentRole?.CanCreateRightsOwners ?? false;
    public static bool CanEditRightsOwners => CurrentRole?.CanEditRightsOwners ?? false;
    public static bool CanDeleteRightsOwners => CurrentRole?.CanDeleteRightsOwners ?? false;
    public static bool CanCreateFilms => CurrentRole?.CanCreateFilms ?? false;
    public static bool CanEditFilmBasicInfo => CurrentRole?.CanEditFilmBasicInfo ?? false;
    public static bool CanEditFilmRightsInfo => CurrentRole?.CanEditFilmRightsInfo ?? false;
    public static bool CanDeleteFilms => CurrentRole?.CanDeleteFilms ?? false;
    public static bool CanManageContacts => CurrentRole?.CanManageContacts ?? false;
    public static bool CanManageSchedule => CurrentRole?.CanManageSchedule ?? false;
    public static bool CanViewContent => CurrentRole?.CanViewContent ?? false;
    public static bool CanViewSchedule => CurrentRole?.CanViewSchedule ?? false;
    public static bool CanViewContacts => CurrentRole?.CanViewContacts ?? false;

    // Combined checks
    public static bool CanEditAnyFilmInfo => CanEditFilmBasicInfo || CanEditFilmRightsInfo;
    public static bool HasAdminAccess => CanManageUsers || CanManageRoles;
}
