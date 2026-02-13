namespace eCommerceMotoRepuestos.Models;

public class CatalogViewModel
{
    public IEnumerable<CategoryViewModel> Categories { get; set; }
    public IEnumerable<ProductViewModel> Products { get; set; }
    public string FilterBy { get; set; }
}
