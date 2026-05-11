using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PKS4.Pr5.Data;
using PKS4.Pr5.ViewModels;

namespace PKS4.Pr5.Controllers;

[ApiController]
[Route("api/calculate")]
public class CalculateApiController : ControllerBase
{
    private readonly ProductionContext context;

    public CalculateApiController(ProductionContext context)
    {
        this.context = context;
    }

    [HttpPost("production")]
    public async Task<IActionResult> CalculateProduction(ProductionCalculationRequest request)
    {
        var product = await context.Products.FirstOrDefaultAsync(item => item.Id == request.ProductId);
        if (product == null)
        {
            return NotFound();
        }

        var minutes = ProductionHelper.CalculateMinutes(product, request.Quantity, 1);
        return Ok(new
        {
            product.Id,
            product.Name,
            request.Quantity,
            Minutes = minutes
        });
    }
}
