using System.ComponentModel.DataAnnotations;

namespace eCommerceMotoRepuestos.Entities;

public class Category
{
    public int CategoryId { get; set; }
    [Required]
    public required string Name { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Product> Products { get; set; } = [];
}
