using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PKS4.Pr5.Data;
using PKS4.Pr5.Models;
using PKS4.Pr5.ViewModels;

namespace PKS4.Pr5.Controllers;

public class ProductsController : Controller
{
    private readonly ProductionContext context;

    public ProductsController(ProductionContext context)
    {
        this.context = context;
    }

    public async Task<IActionResult> Index(string? category)
    {
        var products = context.Products
            .Include(item => item.ProductMaterials)
            .ThenInclude(item => item.Material)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
        {
            products = products.Where(item => item.Category == category);
        }

        ViewBag.Category = category;
        ViewBag.Categories = await context.Products
            .Select(item => item.Category)
            .Distinct()
            .OrderBy(item => item)
            .ToListAsync();

        return View(await products.OrderBy(item => item.Name).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await context.Products
            .Include(item => item.ProductMaterials)
            .ThenInclude(item => item.Material)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        var model = new ProductDetailsViewModel
        {
            Product = product,
            AvailableMaterials = await context.Materials.OrderBy(item => item.Name).ToListAsync()
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Name))
        {
            TempData["Message"] = "Название продукта не заполнено.";
            return RedirectToAction(nameof(Index));
        }

        context.Products.Add(product);
        await context.SaveChangesAsync();
        TempData["Message"] = "Продукт добавлен.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Update(int id, string name, string description, string specifications, string category, int minimalStock, int productionTimePerUnit)
    {
        var product = await context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            TempData["Message"] = "Название продукта не заполнено.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (productionTimePerUnit <= 0)
        {
            TempData["Message"] = "Время производства должно быть больше нуля.";
            return RedirectToAction(nameof(Details), new { id });
        }

        product.Name = name;
        product.Description = description ?? string.Empty;
        product.Specifications = string.IsNullOrWhiteSpace(specifications) ? "{}" : specifications;
        product.Category = category ?? string.Empty;
        product.MinimalStock = Math.Max(0, minimalStock);
        product.ProductionTimePerUnit = productionTimePerUnit;

        var activeOrders = await context.WorkOrders
            .Include(item => item.ProductionLine)
            .Where(item => item.ProductId == id && item.Status != "Completed" && item.Status != "Cancelled" && item.Status != "Returned")
            .ToListAsync();

        foreach (var order in activeOrders)
        {
            var efficiency = order.ProductionLine?.EfficiencyFactor ?? 1;
            order.EstimatedEndDate = ProductionHelper.CalculateEndDate(product, order.Quantity, efficiency, order.StartDate);
        }

        await context.SaveChangesAsync();
        TempData["Message"] = "Продукт обновлен.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> AddMaterial(int productId, int materialId, decimal quantityNeeded)
    {
        if (quantityNeeded <= 0)
        {
            TempData["Message"] = "Количество материала должно быть больше нуля.";
            return RedirectToAction(nameof(Details), new { id = productId });
        }

        var link = await context.ProductMaterials.FindAsync(productId, materialId);
        if (link == null)
        {
            link = new ProductMaterial
            {
                ProductId = productId,
                MaterialId = materialId,
                QuantityNeeded = quantityNeeded
            };
            context.ProductMaterials.Add(link);
        }
        else
        {
            link.QuantityNeeded = quantityNeeded;
        }

        await context.SaveChangesAsync();
        TempData["Message"] = "Материал для продукта сохранен.";
        return RedirectToAction(nameof(Details), new { id = productId });
    }
}
