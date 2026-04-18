using System.ComponentModel.DataAnnotations;

namespace eCommerceMotoRepuestos.Enums;

public enum PaymentType
{
    [Display(Name = "En local")]
    Store = 0,
    [Display(Name = "Tarjeta")]
    Card = 1
}
