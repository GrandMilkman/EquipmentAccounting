namespace EquipmentAccounting.Models;

public class RightsOwner
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DateAdded { get; set; } = DateTime.Now;

    // Contact (company that sells rights)
    public int? ContactId { get; set; }
    public virtual Contact? Contact { get; set; }

    public virtual ICollection<Film> Films { get; set; } = new List<Film>();
}
