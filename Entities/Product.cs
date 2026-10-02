using System.ComponentModel.DataAnnotations;

namespace eCommerceMotoRepuestos.Entities;

public class Product
{
    public int ProductId { get; set; }
    public int CategoryId { get; set; }
    public bool IsActive { get; set; } = true;
    [Required]
    public required string Name { get; set; }
    public required string Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? ImageName { get; set; } = null;
    /// <summary>
    /// Name and description lowercased and without accents, used for catalog search.
    /// Kept in sync by ProductService on add/edit.
    /// </summary>
    public string SearchText { get; set; } = string.Empty;

    public Category? Category { get; set; }
}
