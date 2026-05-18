using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PKS5.Server.Data;
using PKS5.Shared;

namespace PKS5.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly StoreDbContext _db;

    public ProductsController(StoreDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    {
        var products = await _db.Products
            .OrderBy(product => product.Id)
            .ToListAsync();

        return Ok(products);
    }

    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct(Product product)
    {
        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProducts), new { id = product.Id }, product);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProduct(int id, Product product)
    {
        if (id != product.Id)
        {
            return BadRequest("Неверный идентификатор товара.");
        }

        var currentProduct = await _db.Products.FindAsync(id);
        if (currentProduct is null)
        {
            return NotFound();
        }

        currentProduct.Name = product.Name;
        currentProduct.Category = product.Category;
        currentProduct.Price = product.Price;
        currentProduct.Quantity = product.Quantity;
        currentProduct.Description = product.Description;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _db.Products.FindAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
