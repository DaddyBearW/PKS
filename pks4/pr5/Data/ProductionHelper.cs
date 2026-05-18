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
        var normalizedStartDate = DateTime.SpecifyKind(startDate, DateTimeKind.Unspecified);
        var endDate = normalizedStartDate.AddMinutes(CalculateMinutes(product, quantity, efficiencyFactor));
        return DateTime.SpecifyKind(endDate, DateTimeKind.Unspecified);
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

    public static async Task ReturnMaterialsAsync(ProductionContext context, WorkOrder order)
    {
        var items = await context.ProductMaterials
            .Include(item => item.Material)
            .Where(item => item.ProductId == order.ProductId)
            .ToListAsync();

        foreach (var item in items)
        {
            if (item.Material != null)
            {
                var coefficient = GetReturnCoefficient(item.Material.Name);
                item.Material.Quantity += item.QuantityNeeded * order.Quantity * coefficient;
            }
        }
    }

    public static decimal GetReturnCoefficient(string materialName)
    {
        var normalizedName = materialName.Trim().ToLowerInvariant();

        if (normalizedName.Contains("сталь"))
        {
            return 0.15m;
        }

        if (normalizedName.Contains("креп"))
        {
            return 0.75m;
        }

        if (normalizedName.Contains("пласт"))
        {
            return 0.60m;
        }

        return 0m;
    }

    public static async Task<bool> HasLineScheduleConflictAsync(
        ProductionContext context,
        int lineId,
        DateTime startDate,
        DateTime endDate,
        int? ignoredOrderId = null)
    {
        return await context.WorkOrders.AnyAsync(item =>
            item.ProductionLineId == lineId &&
            item.Status != "Completed" &&
            item.Status != "Cancelled" &&
            item.Status != "Returned" &&
            (!ignoredOrderId.HasValue || item.Id != ignoredOrderId.Value) &&
            item.StartDate < endDate &&
            startDate < item.EstimatedEndDate);
    }
}
