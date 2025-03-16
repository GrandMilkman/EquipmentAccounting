namespace EquipmentAccounting.Models;

public class Repair
{
    public int Id { get; set; }
    public string InventoryNumber { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; }
}