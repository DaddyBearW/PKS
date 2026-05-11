using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PKS4.Pr4.Data;

namespace PKS4.Pr4.Controllers;

public class AttractionsController : Controller
{
    private readonly TouristGuideContext context;

    public AttractionsController(TouristGuideContext context)
    {
        this.context = context;
    }

    public async Task<IActionResult> Details(int id)
    {
        var attraction = await context.Attractions
            .Include(item => item.City)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (attraction == null)
        {
            return NotFound();
        }

        return View(attraction);
    }
}
