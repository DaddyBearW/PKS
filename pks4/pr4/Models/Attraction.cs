namespace PKS4.Pr4.Models;

public class Attraction
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string History { get; set; } = string.Empty;
    public string WorkingHours { get; set; } = string.Empty;
    public decimal VisitPrice { get; set; }
    public string PhotoUrl { get; set; } = string.Empty;
    public int CityId { get; set; }
    public City? City { get; set; }
}
