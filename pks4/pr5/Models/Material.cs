using System.ComponentModel.DataAnnotations;

namespace PKS4.Pr5.Models;

public class Material
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public decimal Quantity { get; set; }
    public string UnitOfMeasure { get; set; } = string.Empty;
    public decimal MinimalStock { get; set; }
    public List<ProductMaterial> ProductMaterials { get; set; } = new();
}
