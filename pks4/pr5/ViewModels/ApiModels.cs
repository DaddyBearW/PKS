namespace PKS4.Pr5.ViewModels;

public class MaterialCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal MinStock { get; set; }
}

public class StockUpdateRequest
{
    public decimal Amount { get; set; }
}

public class ProductCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public int ProdTime { get; set; }
    public string Category { get; set; } = string.Empty;
}

public class LineStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

public class OrderCreateRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public int? LineId { get; set; }
}

public class OrderProgressRequest
{
    public int Percent { get; set; }
}

public class ProductionCalculationRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
