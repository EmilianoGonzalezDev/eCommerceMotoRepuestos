namespace eCommerceMotoRepuestos.Models;

public class OrderItemViewModel
{
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }

    public string OrderItemSummary =>
    (Quantity > 1) ? $"{ProductName} | {Quantity} x ${Price:0} (${(Quantity * Price):0})"
                   : $"{ProductName} | {Quantity} x ${Price:0}";
}
