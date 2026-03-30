using eCommerceMotoRepuestos.Models;
using eCommerceMotoRepuestos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eCommerceMotoRepuestos.Controllers;

[Authorize(Roles = "Admin")]
public class ProductController(ProductService _productService) : Controller
{
    private static readonly int[] PageSizes = [5, 10, 15, 20, 50, 100];
    private const int DefaultAdminPageSize = 10;

    private static int NormalizePageSize(int? pageSize, int defaultSize)
    {
        if (pageSize is null) return defaultSize;
        return Array.IndexOf(PageSizes, pageSize.Value) >= 0 ? pageSize.Value : defaultSize;
    }

    public async Task<IActionResult> Index(
        int page = 1,
        int pageSize = DefaultAdminPageSize,
        ProductSortBy sortBy = ProductSortBy.Name,
        SortDirection sortDir = SortDirection.Asc)
    {
        var size = NormalizePageSize(pageSize, DefaultAdminPageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy);
        var normalizedSortDir = NormalizeSortDirection(sortDir);

        var products = await _productService.GetAllAsync();
        var sortedProducts = SortProducts(products, normalizedSortBy, normalizedSortDir);
        var pagedProducts = PagedResult<ProductViewModel>.Create(sortedProducts, page, size);

        var viewModel = new ProductIndexViewModel
        {
            Products = pagedProducts,
            CurrentSortBy = normalizedSortBy,
            CurrentSortDir = normalizedSortDir
        };

        return View(viewModel);
    }

    private static ProductSortBy NormalizeSortBy(ProductSortBy sortBy)
    {
        return Enum.IsDefined(sortBy) ? sortBy : ProductSortBy.Name;
    }

    private static SortDirection NormalizeSortDirection(SortDirection sortDir)
    {
        return Enum.IsDefined(sortDir) ? sortDir : SortDirection.Asc;
    }

    private static IEnumerable<ProductViewModel> SortProducts(
        IEnumerable<ProductViewModel> products,
        ProductSortBy sortBy,
        SortDirection sortDir)
    {
        var isDesc = sortDir == SortDirection.Desc;

        return sortBy switch
        {
            ProductSortBy.Category => isDesc
                ? products.OrderByDescending(x => x.Category.Name).ThenBy(x => x.Name)
                : products.OrderBy(x => x.Category.Name).ThenBy(x => x.Name),
            ProductSortBy.Price => isDesc
                ? products.OrderByDescending(x => x.Price).ThenBy(x => x.Name)
                : products.OrderBy(x => x.Price).ThenBy(x => x.Name),
            ProductSortBy.Stock => isDesc
                ? products.OrderByDescending(x => x.Stock).ThenBy(x => x.Name)
                : products.OrderBy(x => x.Stock).ThenBy(x => x.Name),
            _ => isDesc
                ? products.OrderByDescending(x => x.Name)
                : products.OrderBy(x => x.Name)
        };
    }

    [HttpGet]
    public async Task<IActionResult> Add()
    {
        var productVM = await _productService.GetAddViewModelAsync();
        return View("AddEdit", productVM);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(ProductViewModel entityVM)
    {
        ModelState.Remove("Categories");
        ModelState.Remove("Category.Name");

        if (!ModelState.IsValid)
        {
            entityVM.Category ??= new CategoryViewModel();
            await _productService.PopulateCategoriesAsync(entityVM);
            return View("AddEdit", entityVM);
        }

        await _productService.AddAsync(entityVM);
        entityVM = await _productService.GetAddViewModelAsync();
        TempData["SuccessMessage"] = "Producto creado.";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var productVM = await _productService.GetEditViewModelAsync(id);
        if (productVM is null) return NotFound();
        return View("AddEdit", productVM);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProductViewModel entityVM)
    {
        ModelState.Remove("Categories");
        ModelState.Remove("Category.Name");
        if (!ModelState.IsValid)
        {
            entityVM.Category ??= new CategoryViewModel();
            await _productService.PopulateCategoriesAsync(entityVM);
            return View("AddEdit", entityVM);
        }

        await _productService.EditAsync(entityVM);
        await _productService.PopulateCategoriesAsync(entityVM);
        TempData["SuccessMessage"] = "Producto editado correctamente.";
        return RedirectToAction("Index");
    }


    public async Task<IActionResult> ToggleStatus(int id)
    {
        try
        {
            var isActive = await _productService.ToggleActiveAsync(id);
            TempData["SuccessMessage"] = isActive
                ? "Se volvió a dar de alta el Producto."
                : "Producto dado de baja correctamente.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch
        {
            TempData["ErrorMessage"] = "No se pudo actualizar el producto.";
        }

        return RedirectToAction("Index");
    }
}

