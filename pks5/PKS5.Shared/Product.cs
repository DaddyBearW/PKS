using System.ComponentModel.DataAnnotations;

namespace PKS5.Shared;

public class Product
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Введите название товара")]
    [StringLength(100, ErrorMessage = "Название должно быть до 100 символов")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите категорию")]
    [StringLength(60, ErrorMessage = "Категория должна быть до 60 символов")]
    public string Category { get; set; } = string.Empty;

    [Range(0, 1_000_000, ErrorMessage = "Цена не может быть отрицательной")]
    public decimal Price { get; set; }

    [Range(0, 100_000, ErrorMessage = "Количество не может быть отрицательным")]
    public int Quantity { get; set; }

    [StringLength(250, ErrorMessage = "Описание должно быть до 250 символов")]
    public string? Description { get; set; }
}
