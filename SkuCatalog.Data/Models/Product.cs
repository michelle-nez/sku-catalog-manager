using System.ComponentModel.DataAnnotations;

namespace SkuCatalog.Data.Models;

public class Product
{
    public int Id { get; set; }

    [Required(ErrorMessage = "SKU is required.")]
    [MaxLength(40)]
    [Display(Name = "SKU")]
    public string Sku { get; set; } = string.Empty;

    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    // decimal, not double - money must be exact.
    [Range(0, 999999, ErrorMessage = "Price must be between 0 and 999,999.")]
    public decimal Price { get; set; }

    [Range(0, 100000, ErrorMessage = "Quantity must be between 0 and 100,000.")]
    public int Quantity { get; set; }

    // Retiring a product hides it. It never deletes the row.
    public bool IsActive { get; set; } = true;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    // The foreign key, and the object it points at.
    [Range(1, int.MaxValue, ErrorMessage = "Choose a category.")]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    public Category? Category { get; set; }
}
