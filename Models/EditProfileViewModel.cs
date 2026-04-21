using System.ComponentModel.DataAnnotations;

namespace eCommerceMotoRepuestos.Models;

public class EditProfileViewModel
{
    public int UserId { get; set; }

    public string? FullName { get; set; }

    [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
    public string? Email { get; set; }

    public string? Password { get; set; }

    [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
    public string? RepeatPassword { get; set; }
}
