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
