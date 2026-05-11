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
    public async Task<IActionResult> Update(int id, string status, float efficiencyFactor)
    {
        var line = await context.ProductionLines.FindAsync(id);
        if (line == null)
        {
            return NotFound();
        }

        line.Status = status;
        line.EfficiencyFactor = efficiencyFactor;
        await context.SaveChangesAsync();
        TempData["Message"] = "Параметры линии обновлены.";
        return RedirectToAction(nameof(Index));
    }
}
