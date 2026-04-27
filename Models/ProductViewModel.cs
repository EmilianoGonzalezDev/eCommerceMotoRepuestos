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
    [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Indicar descripción para el producto.")]
    [StringLength(5000, ErrorMessage = "La descripción no puede superar los 5000 caracteres.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Indicar precio.")]
    [Range(typeof(decimal), "0", "9999999999", ErrorMessage = "Indique un precio entre 0 y 9999999999.")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Indicar stock.")]
    [Range(0, 9999999, ErrorMessage = "El stock debe estar entre 0 y 9999999.")]
    public int Stock { get; set; }

    public string? ImageName { get; set; }
    public IFormFile? ImageFile { get; set; }
}
