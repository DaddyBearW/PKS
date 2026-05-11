using PKS4.Pr5.Models;

namespace PKS4.Pr5.Data;

public static class ProductionSeedData
{
    public static void Initialize(ProductionContext context)
    {
        if (context.Products.Any())
        {
            return;
        }

        var steel = new Material
        {
            Name = "Сталь",
            Quantity = 500,
            UnitOfMeasure = "кг",
            MinimalStock = 100
        };

        var plastic = new Material
        {
            Name = "Пластик",
            Quantity = 220,
            UnitOfMeasure = "кг",
            MinimalStock = 80
        };

        var screws = new Material
        {
            Name = "Крепеж",
            Quantity = 1500,
            UnitOfMeasure = "шт",
            MinimalStock = 300
        };

        context.Materials.AddRange(steel, plastic, screws);
        context.SaveChanges();

        var chair = new Product
        {
            Name = "Стул офисный",
            Description = "Простой офисный стул",
            Specifications = "{\"color\":\"black\"}",
            Category = "Мебель",
            MinimalStock = 10,
            ProductionTimePerUnit = 25
        };

        var shelf = new Product
        {
            Name = "Полка настенная",
            Description = "Небольшая полка для дома",
            Specifications = "{\"length\":\"80 cm\"}",
            Category = "Мебель",
            MinimalStock = 7,
            ProductionTimePerUnit = 18
        };

        context.Products.AddRange(chair, shelf);
        context.SaveChanges();

        context.ProductMaterials.AddRange(
            new ProductMaterial { ProductId = chair.Id, MaterialId = steel.Id, QuantityNeeded = 3 },
            new ProductMaterial { ProductId = chair.Id, MaterialId = screws.Id, QuantityNeeded = 8 },
            new ProductMaterial { ProductId = shelf.Id, MaterialId = steel.Id, QuantityNeeded = 2 },
            new ProductMaterial { ProductId = shelf.Id, MaterialId = plastic.Id, QuantityNeeded = 1.5m },
            new ProductMaterial { ProductId = shelf.Id, MaterialId = screws.Id, QuantityNeeded = 4 });

        var line1 = new ProductionLine
        {
            Name = "Линия 1",
            Status = "Active",
            EfficiencyFactor = 1
        };

        var line2 = new ProductionLine
        {
            Name = "Линия 2",
            Status = "Stopped",
            EfficiencyFactor = 0.8f
        };

        context.ProductionLines.AddRange(line1, line2);
        context.SaveChanges();

        var order = new WorkOrder
        {
            ProductId = chair.Id,
            ProductionLineId = line1.Id,
            Quantity = 5,
            StartDate = DateTime.Today.AddHours(9),
            EstimatedEndDate = DateTime.Today.AddHours(11),
            Status = "Pending",
            ProgressPercent = 0
        };

        context.WorkOrders.Add(order);
        context.SaveChanges();
    }
}
