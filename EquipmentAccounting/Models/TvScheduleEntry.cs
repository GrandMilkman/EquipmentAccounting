namespace EquipmentAccounting.Models;

public class TvScheduleEntry
{
    public int Id { get; set; }

    public int FilmId { get; set; }
    public virtual Film Film { get; set; } = null!;

    public DateTime ScheduledDateTime { get; set; }
    public bool IsAired { get; set; } // Has this entry been broadcast

    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
