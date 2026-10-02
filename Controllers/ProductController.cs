using eCommerceMotoRepuestos.Models;
using eCommerceMotoRepuestos.Services;
using eCommerceMotoRepuestos.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eCommerceMotoRepuestos.Controllers;

[Authorize(Roles = "Admin")]
public class ProductController(
    ProductService _productService,
    AppSettingService _appSettingService) : Controller
{
    private static int NormalizePageSize(int? pageSize, int defaultSize)
    {
        if (pageSize is null) return defaultSize;
        return Array.IndexOf(PaginationSettings.PageSizes, pageSize.Value) >= 0 ? pageSize.Value : defaultSize;
    }

    public async Task<IActionResult> Index(
        int page = 1,
        int pageSize = PaginationSettings.DefaultPageSize,
        ProductSortBy sortBy = ProductSortBy.Name,
        SortDirection sortDir = SortDirection.Asc,
        string search = "",
        bool lowStockOnly = false)
    {
        var size = NormalizePageSize(pageSize, PaginationSettings.DefaultPageSize);
        var normalizedSortBy = NormalizeSortBy(sortBy);
        var normalizedSortDir = NormalizeSortDirection(sortDir);
        var normalizedSearch = NormalizeSearch(search);
        var lowStockThreshold = await _appSettingService.GetLowStockThresholdAsync();

        var pagedProducts = await _productService.GetAdminPagedAsync(
            page, size, normalizedSortBy, normalizedSortDir, normalizedSearch, lowStockOnly, lowStockThreshold);

        var viewModel = new ProductIndexViewModel
        {
            Products = pagedProducts,
            CurrentSortBy = normalizedSortBy,
            CurrentSortDir = normalizedSortDir,
            Search = normalizedSearch,
            LowStockOnly = lowStockOnly,
            LowStockThreshold = lowStockThreshold
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

    private static string NormalizeSearch(string? search)
    {
        return string.IsNullOrWhiteSpace(search) ? string.Empty : search.Trim();
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

        var success = await _productService.EditAsync(entityVM);
        if (!success)
        {
            return NotFound();
        }

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

