namespace eCommerceMotoRepuestos.Models;

public class ProductIndexViewModel
{
    public required PagedResult<ProductViewModel> Products { get; init; }
    public ProductSortBy CurrentSortBy { get; init; } = ProductSortBy.Name;
    public SortDirection CurrentSortDir { get; init; } = SortDirection.Asc;
    public string Search { get; init; } = string.Empty;
    public bool LowStockOnly { get; init; }

    public IReadOnlyList<ProductViewModel> Items => Products.Items;
    public int PageNumber => Products.PageNumber;
    public int PageSize => Products.PageSize;
    public int TotalPages => Products.TotalPages;
    public bool HasPrevious => Products.HasPrevious;
    public bool HasNext => Products.HasNext;

    public IReadOnlyList<ProductSortColumnViewModel> SortColumns { get; } =
    [
        new(ProductSortBy.Name, "Nombre", "Orden ascendente", "Orden descendente"),
        new(ProductSortBy.Category, "Categoría", "Orden ascendente", "Orden descendente"),
        new(ProductSortBy.Price, "Precio", "Menor a mayor", "Mayor a menor"),
        new(ProductSortBy.Stock, "Stock", "Menor a mayor", "Mayor a menor")
    ];

    public bool IsActiveSort(ProductSortBy sortBy, SortDirection sortDirection)
    {
        return CurrentSortBy == sortBy && CurrentSortDir == sortDirection;
    }

    public string GetSortButtonCssClass(ProductSortBy sortBy, SortDirection sortDirection)
    {
        return IsActiveSort(sortBy, sortDirection)
            ? "btn btn-secondary active"
            : "btn btn-light-secondary";
    }

    public object GetSortRouteValues(ProductSortBy sortBy, SortDirection sortDirection)
    {
        return new
        {
            page = 1,
            pageSize = PageSize,
            sortBy,
            sortDir = sortDirection,
            search = Search,
            lowStockOnly = LowStockOnly
        };
    }

    public object GetPageRouteValues(int page)
    {
        return new
        {
            page,
            pageSize = PageSize,
            sortBy = CurrentSortBy,
            sortDir = CurrentSortDir,
            search = Search,
            lowStockOnly = LowStockOnly
        };
    }
}

public record ProductSortColumnViewModel(
    ProductSortBy SortBy,
    string Label,
    string AscTitle,
    string DescTitle);
