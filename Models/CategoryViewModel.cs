using System.ComponentModel.DataAnnotations;

namespace eCommerceMotoRepuestos.Models;

public class CategoryViewModel
{
    public int CategoryId { get; set; }
    [Required]
    public string Name { get; set; }
}
