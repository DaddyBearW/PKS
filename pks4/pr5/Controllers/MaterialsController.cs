using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PKS4.Pr5.Data;
using PKS4.Pr5.Models;

namespace PKS4.Pr5.Controllers;

public class MaterialsController : Controller
{
    private readonly ProductionContext context;

    public MaterialsController(ProductionContext context)
    {
        this.context = context;
    }

    public async Task<IActionResult> Index()
    {
        var materials = await context.Materials
            .OrderBy(item => item.Name)
            .ToListAsync();

        return View(materials);
    }

    [HttpPost]
    public async Task<IActionResult> Add(Material material)
    {
        if (string.IsNullOrWhiteSpace(material.Name))
        {
            TempData["Message"] = "Название материала не заполнено.";
            return RedirectToAction(nameof(Index));
        }

        context.Materials.Add(material);
        await context.SaveChangesAsync();
        TempData["Message"] = "Материал добавлен.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Restock(int id, decimal amount)
    {
        if (amount <= 0)
        {
            TempData["Message"] = "Количество для пополнения должно быть больше нуля.";
            return RedirectToAction(nameof(Index));
        }

        var material = await context.Materials.FindAsync(id);
        if (material == null)
        {
            return NotFound();
        }

        material.Quantity += amount;
        await context.SaveChangesAsync();
        TempData["Message"] = "Количество обновлено.";
        return RedirectToAction(nameof(Index));
    }
}
