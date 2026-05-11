using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PKS4.Pr5.Data;
using PKS4.Pr5.ViewModels;

namespace PKS4.Pr5.Controllers;

[ApiController]
[Route("api/lines")]
public class LinesApiController : ControllerBase
{
    private readonly ProductionContext context;

    public LinesApiController(ProductionContext context)
    {
        this.context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool available = false)
    {
        var query = context.ProductionLines.AsQueryable();
        if (available)
        {
            query = query.Where(item => item.Status == "Active" && item.CurrentWorkOrderId == null);
        }

        return Ok(await query.OrderBy(item => item.Name).ToListAsync());
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, LineStatusRequest request)
    {
        var line = await context.ProductionLines.FindAsync(id);
        if (line == null)
        {
            return NotFound();
        }

        line.Status = request.Status;
        await context.SaveChangesAsync();
        return Ok(line);
    }

    [HttpGet("{id}/schedule")]
    public async Task<IActionResult> GetSchedule(int id)
    {
        var orders = await context.WorkOrders
            .Include(item => item.Product)
            .Where(item => item.ProductionLineId == id)
            .OrderBy(item => item.StartDate)
            .Select(item => new
            {
                item.Id,
                Product = item.Product!.Name,
                item.Quantity,
                item.StartDate,
                item.EstimatedEndDate,
                item.Status
            })
            .ToListAsync();

        return Ok(orders);
    }
}
