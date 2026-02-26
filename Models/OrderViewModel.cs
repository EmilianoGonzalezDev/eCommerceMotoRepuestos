using eCommerceMotoRepuestos.Entities;

namespace eCommerceMotoRepuestos.Models;

public class OrderViewModel
{
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public ICollection<OrderItemViewModel>? OrderItems { get; set; }

    public string OrderDateFormatted => OrderDate.ToString("dd/MM/yyyy HH:mm");
    public string TotalAmountFormatted => TotalAmount.ToString("C");
}
