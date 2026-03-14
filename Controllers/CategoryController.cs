using eCommerceMotoRepuestos.Entities;
using eCommerceMotoRepuestos.Models;
using eCommerceMotoRepuestos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eCommerceMotoRepuestos.Controllers;

[Authorize(Roles = "Admin")]
public class CategoryController(CategoryService _categoryService) : Controller
{
    private static readonly int[] PageSizes = [5, 10, 15, 20, 50, 100];
    private const int DefaultAdminPageSize = 10;

    private static int NormalizePageSize(int? pageSize, int defaultSize)
    {
        if (pageSize is null) return defaultSize;
        return Array.IndexOf(PageSizes, pageSize.Value) >= 0 ? pageSize.Value : defaultSize;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = DefaultAdminPageSize)
    {
        var size = NormalizePageSize(pageSize, DefaultAdminPageSize);
        var categories = await _categoryService.GetAllAsync();
        var pagedCategories = PagedResult<CategoryViewModel>.Create(categories, page, size);
        return View(pagedCategories);
    }

    [HttpGet]
    public IActionResult Add()
    {
        return View("AddEdit", new CategoryViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(CategoryViewModel viewModel)
    {
        if (!ModelState.IsValid) return View("AddEdit", viewModel);

        await _categoryService.AddAsync(viewModel);
        TempData["SuccessMessage"] = "Categoría creada correctamente.";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var categoryViewModel = await _categoryService.GetEditViewModelAsync(id);
        if (categoryViewModel is null) return NotFound();
        return View("AddEdit", categoryViewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CategoryViewModel viewModel)
    {
        if (!ModelState.IsValid) return View("AddEdit", viewModel);
        await _categoryService.EditAsync(viewModel);
        TempData["SuccessMessage"] = "Categoría editada correctamente.";
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> ToggleStatus(int id)
    {
        try
        {
            var isActive = await _categoryService.ToggleActiveAsync(id);
            TempData["SuccessMessage"] = isActive ? "Se volvió a dar de alta la categoría." : "Categoría dada de baja correctamente.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch
        {
            TempData["ErrorMessage"] = "No se pudo actualizar la categoría.";
        }

        return RedirectToAction("Index");
    }

}





