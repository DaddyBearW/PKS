using PKS4.Pr5.Models;

namespace PKS4.Pr5.ViewModels;

public class ProductDetailsViewModel
{
    public Product Product { get; set; } = new();
    public List<Material> AvailableMaterials { get; set; } = new();
}
