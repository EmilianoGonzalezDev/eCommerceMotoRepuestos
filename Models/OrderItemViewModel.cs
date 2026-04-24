namespace eCommerceMotoRepuestos.Models;

public class OrderItemViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }

    public string FormattedPrice => Quantity > 1
    ? $"${Price:0} c/u"
    : $"${Price:0}";
}
