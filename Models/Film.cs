namespace EquipmentAccounting.Models;

public class Film
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string AgeRestriction { get; set; } = string.Empty;
    public int Duration { get; set; } // in minutes
    public string FilePath { get; set; } = string.Empty;

    // Rights information
    public DateTime? PurchaseDate { get; set; }
    public DateTime? RightsExpirationDate { get; set; }
    public int ShowCount { get; set; } // Number of allowed showings remaining

    public DateTime DateAdded { get; set; } = DateTime.Now;

    // Foreign key to RightsOwner
    public int RightsOwnerId { get; set; }
    public virtual RightsOwner RightsOwner { get; set; } = null!;

    public virtual ICollection<TvScheduleEntry> ScheduleEntries { get; set; } = new List<TvScheduleEntry>();

    // Helper property to check if rights are valid
    public bool HasValidRights =>
        (!RightsExpirationDate.HasValue || RightsExpirationDate.Value >= DateTime.Today)
        && ShowCount > 0;
}
