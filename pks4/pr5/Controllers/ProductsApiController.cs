using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PKS4.Pr5.Data;
using PKS4.Pr5.Models;
using PKS4.Pr5.ViewModels;

namespace PKS4.Pr5.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsApiController : ControllerBase
{
    private readonly ProductionContext context;

    public ProductsApiController(ProductionContext context)
    {
        this.context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? category)
    {
        var query = context.Products.AsQueryable();
        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(item => item.Category == category);
        }

        return Ok(await query.OrderBy(item => item.Name).ToListAsync());
    }

    [HttpGet("{id}/materials")]
    public async Task<IActionResult> GetMaterials(int id)
    {
        var materials = await context.ProductMaterials
            .Include(item => item.Material)
            .Where(item => item.ProductId == id)
            .Select(item => new
            {
                item.MaterialId,
                MaterialName = item.Material!.Name,
                item.QuantityNeeded,
                Unit = item.Material.UnitOfMeasure
            })
            .ToListAsync();

        return Ok(materials);
    }

    [HttpPost]
    public async Task<IActionResult> Create(ProductCreateRequest request)
    {
        var product = new Product
        {
            Name = request.Name,
            ProductionTimePerUnit = request.ProdTime,
            Category = request.Category,
            Description = string.Empty,
            Specifications = "{}"
        };

        context.Products.Add(product);
        await context.SaveChangesAsync();
        return Ok(product);
    }
}
