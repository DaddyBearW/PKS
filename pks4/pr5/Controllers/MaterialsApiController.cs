using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PKS4.Pr5.Data;
using PKS4.Pr5.Models;
using PKS4.Pr5.ViewModels;

namespace PKS4.Pr5.Controllers;

[ApiController]
[Route("api/materials")]
public class MaterialsApiController : ControllerBase
{
    private readonly ProductionContext context;

    public MaterialsApiController(ProductionContext context)
    {
        this.context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery(Name = "low_stock")] bool lowStock = false)
    {
        var query = context.Materials.AsQueryable();
        if (lowStock)
        {
            query = query.Where(item => item.Quantity <= item.MinimalStock);
        }

        return Ok(await query.OrderBy(item => item.Name).ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create(MaterialCreateRequest request)
    {
        var material = new Material
        {
            Name = request.Name,
            Quantity = request.Quantity,
            UnitOfMeasure = request.Unit,
            MinimalStock = request.MinStock
        };

        context.Materials.Add(material);
        await context.SaveChangesAsync();
        return Ok(material);
    }

    [HttpPut("{id}/stock")]
    public async Task<IActionResult> UpdateStock(int id, StockUpdateRequest request)
    {
        var material = await context.Materials.FindAsync(id);
        if (material == null)
        {
            return NotFound();
        }

        material.Quantity += request.Amount;
        await context.SaveChangesAsync();
        return Ok(material);
    }
}
