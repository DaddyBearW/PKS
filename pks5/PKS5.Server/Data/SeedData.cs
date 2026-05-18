using PKS5.Shared;

namespace PKS5.Server.Data;

public static class SeedData
{
    public static void Initialize(StoreDbContext db)
    {
        if (db.Products.Any())
        {
            return;
        }

        db.Products.AddRange(
            new Product
            {
                Name = "Ноутбук",
                Category = "Техника",
                Price = 59990,
                Quantity = 5,
                Description = "Учебный пример товара для интернет-магазина"
            },
            new Product
            {
                Name = "Мышь",
                Category = "Аксессуары",
                Price = 1490,
                Quantity = 20,
                Description = "Беспроводная мышь"
            },
            new Product
            {
                Name = "Наушники",
                Category = "Аудио",
                Price = 3290,
                Quantity = 12,
                Description = "Проводные наушники"
            });

        db.SaveChanges();
    }
}
