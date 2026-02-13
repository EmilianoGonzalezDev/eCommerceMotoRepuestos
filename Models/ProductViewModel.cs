using eCommerceMotoRepuestos.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace eCommerceMotoRepuestos.Models;

public class ProductViewModel
{
    public int ProductId { get; set; }
    public CategoryViewModel Category { get; set; }
    public List<SelectListItem> Categories { get; set; }

    [Required]
    public string Name { get; set; }
    [Required]
    public string Description { get; set; }
    [Required]
    public decimal Price { get; set; }
    [Required]
    public int Stock { get; set; }
    public string? ImageName { get; set; } = null;

    public IFormFile? ImageFile { get; set; }
}
