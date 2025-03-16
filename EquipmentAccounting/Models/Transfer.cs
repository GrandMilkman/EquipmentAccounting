namespace EquipmentAccounting.Models;

public class Transfer
{
    public int Id { get; set; }
    public string InventoryNumber { get; set; }
    public DateTime Date { get; set; }
    public string NewLocation { get; set; }
}