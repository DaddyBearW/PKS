using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PKS4.Pr5.Data;

namespace PKS4.Pr5.Controllers;

public class LinesController : Controller
{
    private readonly ProductionContext context;

    public LinesController(ProductionContext context)
    {
        this.context = context;
    }

    public async Task<IActionResult> Index()
    {
        var lines = await context.ProductionLines
            .Include(item => item.WorkOrders)
            .ThenInclude(item => item.Product)
            .OrderBy(item => item.Name)
            .ToListAsync();

        return View(lines);
    }

    [HttpPost]
    public async Task<IActionResult> Update(int id, string status, string efficiencyFactor)
    {
        var line = await context.ProductionLines
            .Include(item => item.WorkOrders)
            .ThenInclude(item => item.Product)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (line == null)
        {
            return NotFound();
        }

        var normalizedEfficiency = (efficiencyFactor ?? string.Empty).Replace(',', '.');
        if (!float.TryParse(normalizedEfficiency, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedEfficiency) || parsedEfficiency <= 0)
        {
            TempData["Message"] = "Коэффициент эффективности должен быть положительным числом.";
            return RedirectToAction(nameof(Index));
        }

        line.Status = status;
        line.EfficiencyFactor = parsedEfficiency;

        foreach (var order in line.WorkOrders.Where(item => item.Status != "Completed" && item.Status != "Cancelled" && item.Status != "Returned"))
        {
            if (order.Product != null)
            {
                order.EstimatedEndDate = ProductionHelper.CalculateEndDate(order.Product, order.Quantity, parsedEfficiency, order.StartDate);
            }
        }

        await context.SaveChangesAsync();
        TempData["Message"] = "Параметры линии обновлены.";
        return RedirectToAction(nameof(Index));
    }
}
