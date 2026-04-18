using eCommerceMotoRepuestos.Enums;

namespace eCommerceMotoRepuestos.Models;

public class OrderViewModel
{
    public DateTime OrderDate { get; set; }
    public int OrderId { get; set; }
    public string? CustomerName { get; set; }
    public decimal TotalAmount { get; set; }
    public PaymentType PaymentType { get; set; }
    public OrderStatus Status { get; set; }
    public ICollection<OrderItemViewModel>? OrderItems { get; set; }

    public string OrderDateFormatted => OrderDate.ToString("dd/MM/yyyy HH:mm");
    public string TotalAmountFormatted => TotalAmount.ToString("C");
}
