namespace eCommerceMotoRepuestos.Models;

public class CatalogViewModel
{
    public IEnumerable<CategoryViewModel> Categories { get; set; }
    public PagedResult<ProductViewModel> Products { get; set; }
    public string FilterBy { get; set; }
    public int CategoryFilterId { get; set; }
    public string? SearchValue { get; set; }
}
