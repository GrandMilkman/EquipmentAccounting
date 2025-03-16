namespace EquipmentAccounting.Models;

public class FixedAsset
{
    public int Id { get; set; }
    public string InventoryNumber { get; set; }
    public int Year { get; set; }
    public string ActNumber { get; set; }
    public string Location { get; set; }
    public decimal Cost { get; set; }
    public int DepreciationRate { get; set; }
    public string Description { get; set; }
    public string ResponsibleName { get; set; }
}