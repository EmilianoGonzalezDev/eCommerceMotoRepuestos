using System.ComponentModel.DataAnnotations;

namespace eCommerceMotoRepuestos.Models;

public class EditProfileViewModel
{
    public int UserId { get; set; }

    [StringLength(150, ErrorMessage = "Este campo no puede superar los 150 caracteres.")]
    public string? FullName { get; set; }

    [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
    [StringLength(150, ErrorMessage = "Este campo no puede superar los 150 caracteres.")]
    public string? Email { get; set; }

    [StringLength(150, ErrorMessage = "Este campo no puede superar los 150 caracteres.")]
    public string? Password { get; set; }

    [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
    [StringLength(150, ErrorMessage = "Este campo no puede superar los 150 caracteres.")]
    public string? RepeatPassword { get; set; }
}
