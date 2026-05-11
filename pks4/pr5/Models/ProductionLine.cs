using System.ComponentModel.DataAnnotations;

namespace PKS4.Pr5.Models;

public class ProductionLine
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string Status { get; set; } = "Stopped";
    public float EfficiencyFactor { get; set; } = 1;
    public int? CurrentWorkOrderId { get; set; }
    public List<WorkOrder> WorkOrders { get; set; } = new();
}
