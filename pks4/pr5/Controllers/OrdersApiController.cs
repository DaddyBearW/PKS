using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PKS4.Pr5.Data;
using PKS4.Pr5.Models;
using PKS4.Pr5.ViewModels;

namespace PKS4.Pr5.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersApiController : ControllerBase
{
    private readonly ProductionContext context;

    public OrdersApiController(ProductionContext context)
    {
        this.context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status, [FromQuery] string? date)
    {
        var query = context.WorkOrders
            .Include(item => item.Product)
            .Include(item => item.ProductionLine)
            .AsQueryable();

        if (status == "active")
        {
            query = query.Where(item => item.Status == "Pending" || item.Status == "InProgress");
        }

        if (date == "today")
        {
            var today = DateTime.Today;
            query = query.Where(item => item.StartDate.Date == today);
        }

        return Ok(await query.OrderByDescending(item => item.Id).ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create(OrderCreateRequest request)
    {
        if (request.Quantity <= 0)
        {
            return BadRequest(new { message = "Количество должно быть больше нуля." });
        }

        var product = await context.Products.FindAsync(request.ProductId);
        if (product == null)
        {
            return NotFound();
        }

        var materialError = await ProductionHelper.CheckMaterialsAsync(context, request.ProductId, request.Quantity);
        if (materialError != null)
        {
            return BadRequest(new { message = materialError });
        }

        float efficiency = 1;
        if (request.LineId.HasValue)
        {
            var line = await context.ProductionLines.FindAsync(request.LineId.Value);
            if (line != null)
            {
                efficiency = line.EfficiencyFactor;
            }
        }

        var startDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
        await ProductionHelper.ReserveMaterialsAsync(context, request.ProductId, request.Quantity);

        var order = new WorkOrder
        {
            ProductId = request.ProductId,
            ProductionLineId = request.LineId,
            Quantity = request.Quantity,
            StartDate = startDate,
            EstimatedEndDate = ProductionHelper.CalculateEndDate(product, request.Quantity, efficiency, startDate),
            Status = "Pending",
            ProgressPercent = 0
        };

        context.WorkOrders.Add(order);
        await context.SaveChangesAsync();
        return Ok(order);
    }

    [HttpPut("{id}/progress")]
    public async Task<IActionResult> UpdateProgress(int id, OrderProgressRequest request)
    {
        var order = await context.WorkOrders.FindAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        order.ProgressPercent = Math.Clamp(request.Percent, 0, 100);
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

        await context.SaveChangesAsync();
        return Ok(order);
    }

    [HttpGet("{id}/details")]
    public async Task<IActionResult> Details(int id)
    {
        var order = await context.WorkOrders
            .Include(item => item.Product)
            .Include(item => item.ProductionLine)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (order == null)
        {
            return NotFound();
        }

        return Ok(order);
    }
}
