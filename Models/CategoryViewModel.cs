using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace eCommerceMotoRepuestos.Models;

public class CategoryViewModel
{
    [Required(ErrorMessage = "Seleccionar una categoría.")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Indicar un nombre.")]
    [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
    [Remote("ValidateName", "Category", AdditionalFields = nameof(CategoryId), ErrorMessage = "Ya existe una categoría con ese nombre.")]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}
