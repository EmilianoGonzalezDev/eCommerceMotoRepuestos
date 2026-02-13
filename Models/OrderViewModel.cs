using eCommerceMotoRepuestos.Entities;

namespace eCommerceMotoRepuestos.Models;

public class OrderViewModel
{
    public string OrderDate { get; set; }
    public string TotalAmount { get; set; }
    public ICollection<OrderItemViewModel>? OrderItems { get; set; }
}
