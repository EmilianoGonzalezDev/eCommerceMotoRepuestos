using System.ComponentModel.DataAnnotations;

namespace eCommerceMotoRepuestos.Models;

public class UserViewModel
{
    public int UserId { get; set; }
    [Required(ErrorMessage = "Completar este campo.")]
    public string FullName { get; set; }
    [Required(ErrorMessage = "Completar este campo.")]
    [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
    public string Email { get; set; }
    [Required(ErrorMessage = "Completar este campo.")]
    public string Password { get; set; }
    [Required(ErrorMessage = "Completar este campo.")]
    public string Type { get; set; }

    [Required(ErrorMessage = "Completar este campo.")]
    public string RepeatPassword { get; set; }
}
