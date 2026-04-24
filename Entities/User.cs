using System.ComponentModel.DataAnnotations;

namespace eCommerceMotoRepuestos.Entities;

public class User
{
    public int UserId { get; set; }
    [Required]
    public required string FullName { get; set; }
    [Required]
    public required string Email { get; set; }
    [Required]
    public required string Password { get; set; }
    [Required]
    public required string Type { get; set; }

    public ICollection<Order> Orders { get; set; } = [];
}