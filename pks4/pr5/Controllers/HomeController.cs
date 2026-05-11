using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PKS4.Pr5.Data;
using PKS4.Pr5.ViewModels;

namespace PKS4.Pr5.Controllers;

public class HomeController : Controller
{
    private readonly ProductionContext context;

    public HomeController(ProductionContext context)
    {
        this.context = context;
    }

    public async Task<IActionResult> Index()
    {
        var model = new HomeDashboardViewModel
        {
            ProductCount = await context.Products.CountAsync(),
            MaterialCount = await context.Materials.CountAsync(),
            ActiveOrdersCount = await context.WorkOrders.CountAsync(item => item.Status == "Pending" || item.Status == "InProgress"),
            LowStockCount = await context.Materials.CountAsync(item => item.Quantity <= item.MinimalStock),
            LatestOrders = await context.WorkOrders
                .Include(item => item.Product)
                .Include(item => item.ProductionLine)
                .OrderByDescending(item => item.Id)
                .Take(5)
                .ToListAsync()
        };

        return View(model);
    }
}
