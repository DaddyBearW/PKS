using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PKS4.Pr4.Data;

namespace PKS4.Pr4.Controllers;

public class CitiesController : Controller
{
    private readonly TouristGuideContext context;

    public CitiesController(TouristGuideContext context)
    {
        this.context = context;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var cities = context.Cities.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            cities = cities.Where(city => city.Name.ToLower().Contains(search.ToLower()));
        }

        ViewBag.Search = search;
        return View(await cities.OrderBy(city => city.Name).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var city = await context.Cities
            .Include(item => item.Attractions)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (city == null)
        {
            return NotFound();
        }

        return View(city);
    }
}
