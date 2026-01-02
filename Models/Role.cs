namespace EquipmentAccounting.Models;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Permissions
    public bool CanManageUsers { get; set; }
    public bool CanManageRoles { get; set; }
    public bool CanCreateRightsOwners { get; set; }
    public bool CanEditRightsOwners { get; set; }
    public bool CanDeleteRightsOwners { get; set; }
    public bool CanCreateFilms { get; set; }
    public bool CanEditFilmBasicInfo { get; set; } // Title, AgeRestriction, Duration, FilePath
    public bool CanEditFilmRightsInfo { get; set; } // PurchaseDate, RightsExpirationDate, ShowCount
    public bool CanDeleteFilms { get; set; }
    public bool CanManageContacts { get; set; }
    public bool CanManageSchedule { get; set; }
    public bool CanViewContent { get; set; }
    public bool CanViewSchedule { get; set; }
    public bool CanViewContacts { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
