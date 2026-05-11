using Microsoft.EntityFrameworkCore;
using PKS4.Pr5.Models;

namespace PKS4.Pr5.Data;

public static class ProductionHelper
{
    public static int CalculateMinutes(Product product, int quantity, float efficiencyFactor)
    {
        if (efficiencyFactor <= 0)
        {
            efficiencyFactor = 1;
        }

        var result = (quantity * product.ProductionTimePerUnit) / efficiencyFactor;
        return (int)Math.Ceiling(result);
    }

    public static DateTime CalculateEndDate(Product product, int quantity, float efficiencyFactor, DateTime startDate)
    {
        return startDate.AddMinutes(CalculateMinutes(product, quantity, efficiencyFactor));
    }

    public static async Task<string?> CheckMaterialsAsync(ProductionContext context, int productId, int quantity)
    {
        var items = await context.ProductMaterials
            .Include(item => item.Material)
            .Where(item => item.ProductId == productId)
            .ToListAsync();

        foreach (var item in items)
        {
            var needed = item.QuantityNeeded * quantity;
            if (item.Material == null || item.Material.Quantity < needed)
            {
                return $"Не хватает материала: {item.Material?.Name ?? "Неизвестно"}";
            }
        }

        return null;
    }

    public static async Task ReserveMaterialsAsync(ProductionContext context, int productId, int quantity)
    {
        var items = await context.ProductMaterials
            .Include(item => item.Material)
            .Where(item => item.ProductId == productId)
            .ToListAsync();

        foreach (var item in items)
        {
            if (item.Material != null)
            {
                item.Material.Quantity -= item.QuantityNeeded * quantity;
            }
        }
    }
}
