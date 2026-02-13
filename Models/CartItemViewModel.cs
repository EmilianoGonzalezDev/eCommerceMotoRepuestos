namespace eCommerceMotoRepuestos.Models;

public class CartItemViewModel
{
    public int ProductId { get; set; }
    public string ImageName { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
