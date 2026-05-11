using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PKS4.Pr5.Data;
using PKS4.Pr5.Models;

namespace PKS4.Pr5.Controllers;

public class OrdersController : Controller
{
    private readonly ProductionContext context;

    public OrdersController(ProductionContext context)
    {
        this.context = context;
    }

    public async Task<IActionResult> Index(string? status)
    {
        var orders = context.WorkOrders
            .Include(item => item.Product)
            .Include(item => item.ProductionLine)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            orders = orders.Where(item => item.Status == status);
        }

        ViewBag.Status = status;
        ViewBag.Products = await context.Products.OrderBy(item => item.Name).ToListAsync();
        ViewBag.Lines = await context.ProductionLines
            .Where(item => item.CurrentWorkOrderId == null)
            .OrderBy(item => item.Name)
            .ToListAsync();

        return View(await orders.OrderByDescending(item => item.Id).ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create(int productId, int quantity, int? lineId, DateTime startDate)
    {
        if (quantity <= 0)
        {
            TempData["Message"] = "Количество должно быть больше нуля.";
            return RedirectToAction(nameof(Index));
        }

        var product = await context.Products.FindAsync(productId);
        if (product == null)
        {
            return NotFound();
        }

        var materialError = await ProductionHelper.CheckMaterialsAsync(context, productId, quantity);
        if (materialError != null)
        {
            TempData["Message"] = materialError;
            return RedirectToAction(nameof(Index));
        }

        float efficiency = 1;
        if (lineId.HasValue)
        {
            var selectedLine = await context.ProductionLines.FindAsync(lineId.Value);
            if (selectedLine != null)
            {
                efficiency = selectedLine.EfficiencyFactor;
            }
        }

        await ProductionHelper.ReserveMaterialsAsync(context, productId, quantity);

        var normalizedStartDate = DateTime.SpecifyKind(startDate, DateTimeKind.Unspecified);

        var order = new WorkOrder
        {
            ProductId = productId,
            ProductionLineId = lineId,
            Quantity = quantity,
            StartDate = normalizedStartDate,
            EstimatedEndDate = ProductionHelper.CalculateEndDate(product, quantity, efficiency, normalizedStartDate),
            Status = "Pending",
            ProgressPercent = 0
        };

        context.WorkOrders.Add(order);
        await context.SaveChangesAsync();
        TempData["Message"] = "Заказ создан.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Start(int id)
    {
        var order = await context.WorkOrders.FindAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        order.Status = "InProgress";
        if (order.ProductionLineId.HasValue)
        {
            var line = await context.ProductionLines.FindAsync(order.ProductionLineId.Value);
            if (line != null)
            {
                line.Status = "Active";
                line.CurrentWorkOrderId = order.Id;
            }
        }

        await context.SaveChangesAsync();
        TempData["Message"] = "Заказ запущен.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Cancel(int id)
    {
        var order = await context.WorkOrders.FindAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        order.Status = "Cancelled";
        if (order.ProductionLineId.HasValue)
        {
            var line = await context.ProductionLines.FindAsync(order.ProductionLineId.Value);
            if (line != null && line.CurrentWorkOrderId == order.Id)
            {
                line.CurrentWorkOrderId = null;
            }
        }

        await context.SaveChangesAsync();
        TempData["Message"] = "Заказ отменен.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Complete(int id)
    {
        var order = await context.WorkOrders.FindAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        order.Status = "Completed";
        order.ProgressPercent = 100;

        if (order.ProductionLineId.HasValue)
        {
            var line = await context.ProductionLines.FindAsync(order.ProductionLineId.Value);
            if (line != null && line.CurrentWorkOrderId == order.Id)
            {
                line.CurrentWorkOrderId = null;
            }
        }

        await context.SaveChangesAsync();
        TempData["Message"] = "Заказ выполнен.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> UpdateProgress(int id, int progressPercent)
    {
        var order = await context.WorkOrders.FindAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        order.ProgressPercent = Math.Clamp(progressPercent, 0, 100);
        if (order.ProgressPercent == 100)
        {
            order.Status = "Completed";
            if (order.ProductionLineId.HasValue)
            {
                var line = await context.ProductionLines.FindAsync(order.ProductionLineId.Value);
                if (line != null && line.CurrentWorkOrderId == order.Id)
                {
                    line.CurrentWorkOrderId = null;
                }
            }
        }
        else if (order.ProgressPercent > 0 && order.Status == "Pending")
        {
            order.Status = "InProgress";
        }

        await context.SaveChangesAsync();
        TempData["Message"] = "Прогресс обновлен.";
        return RedirectToAction(nameof(Index));
    }
}
