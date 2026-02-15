using System.ComponentModel.DataAnnotations;

namespace eCommerceMotoRepuestos.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
    public string Email { get; set; }
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    public string Password { get; set; }
}
