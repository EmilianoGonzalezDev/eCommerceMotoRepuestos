using eCommerceMotoRepuestos.Models;
using eCommerceMotoRepuestos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eCommerceMotoRepuestos.Controllers;

[Authorize(Roles = "Admin")]
public class ProductController(ProductService _productService) : Controller
{
    private const int AdminPageSize = 10;

    public async Task<IActionResult> Index(int page = 1)
    {
        var products = await _productService.GetAllAsync();
        var pagedProducts = PagedResult<ProductViewModel>.Create(products, page, AdminPageSize);
        return View(pagedProducts);
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
