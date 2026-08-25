using System.ComponentModel.DataAnnotations;

namespace SkuCatalog.Data.Models;

public class Category
{
    public int Id { get; set; }

    [Required]
    [MaxLength(60)]
    public string Name { get; set; } = string.Empty;

    // One category has many products. EF Core reads this to build the relationship.
    public List<Product> Products { get; set; } = new();
}
