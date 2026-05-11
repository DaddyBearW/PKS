using System.ComponentModel.DataAnnotations;

namespace PKS4.Pr5.Models;

public class Product
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
    public string Specifications { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int MinimalStock { get; set; }
    public int ProductionTimePerUnit { get; set; }
    public List<ProductMaterial> ProductMaterials { get; set; } = new();
    public List<WorkOrder> WorkOrders { get; set; } = new();
}
