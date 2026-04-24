using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace eCommerceMotoRepuestos.Models;

public class ProductViewModel
{
    public int ProductId { get; set; }
    public bool IsActive { get; set; }
    public CategoryViewModel Category { get; set; } = new();
    public List<SelectListItem> Categories { get; set; } = [];

    [Required(ErrorMessage = "Indicar un nombre.")]
    public string Name { get; set; } = string.Empty;
    [Required(ErrorMessage = "Indicar descripción para el producto.")]
    public string Description { get; set; } = string.Empty;
    [Required(ErrorMessage = "Indicar precio.")]
    public decimal Price { get; set; }
    [Required(ErrorMessage = "Indicar stock.")]
    public int Stock { get; set; }
    public string? ImageName { get; set; }
    public IFormFile? ImageFile { get; set; }
}
