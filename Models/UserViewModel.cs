using System.ComponentModel.DataAnnotations;

namespace eCommerceMotoRepuestos.Models;

public class UserViewModel
{
    public int UserId { get; set; }

    [Required(ErrorMessage = "Completar este campo.")]
    [StringLength(150, ErrorMessage = "Este campo no puede superar los 150 caracteres.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Completar este campo.")]
    [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
    [StringLength(150, ErrorMessage = "Este campo no puede superar los 150 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Completar este campo.")]
    [StringLength(150, ErrorMessage = "Este campo no puede superar los 150 caracteres.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Completar este campo.")]
    [StringLength(150, ErrorMessage = "Este campo no puede superar los 150 caracteres.")]
    public string Type { get; set; } = string.Empty;

    [Required(ErrorMessage = "Completar este campo.")]
    [StringLength(150, ErrorMessage = "Este campo no puede superar los 150 caracteres.")]
    public string RepeatPassword { get; set; } = string.Empty;
}
