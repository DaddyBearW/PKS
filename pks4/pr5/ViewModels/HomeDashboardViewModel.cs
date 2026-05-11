using PKS4.Pr5.Models;

namespace PKS4.Pr5.ViewModels;

public class HomeDashboardViewModel
{
    public int ProductCount { get; set; }
    public int MaterialCount { get; set; }
    public int ActiveOrdersCount { get; set; }
    public int LowStockCount { get; set; }
    public List<WorkOrder> LatestOrders { get; set; } = new();
}
