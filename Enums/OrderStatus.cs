using System.ComponentModel.DataAnnotations;

namespace eCommerceMotoRepuestos.Enums;

public enum OrderStatus
{
    [Display(Name = "Pendiente")]
    Pending = 0,
    [Display(Name = "Preparado")]
    Prepared = 1,
    [Display(Name = "Entregado")]
    Delivered = 2,
    [Display(Name = "Cancelado")]
    Cancelled = 3
}
