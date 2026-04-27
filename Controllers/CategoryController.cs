using eCommerceMotoRepuestos.Models;
using eCommerceMotoRepuestos.Services;
using eCommerceMotoRepuestos.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eCommerceMotoRepuestos.Controllers;

[Authorize(Roles = "Admin")]
public class CategoryController(CategoryService _categoryService) : Controller
{
    private static int NormalizePageSize(int? pageSize, int defaultSize)
    {
        if (pageSize is null) return defaultSize;
        return Array.IndexOf(PaginationSettings.PageSizes, pageSize.Value) >= 0 ? pageSize.Value : defaultSize;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = PaginationSettings.DefaultPageSize)
    {
        var size = NormalizePageSize(pageSize, PaginationSettings.DefaultPageSize);
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
        viewModel.Name = (viewModel.Name ?? string.Empty).Trim();

        if (await _categoryService.ExistsByNameAsync(viewModel.Name))
        {
            ModelState.AddModelError(nameof(CategoryViewModel.Name), "Ya existe una categoría con ese nombre.");
        }

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
        viewModel.Name = (viewModel.Name ?? string.Empty).Trim();

        if (await _categoryService.ExistsByNameAsync(viewModel.Name, viewModel.CategoryId))
        {
            ModelState.AddModelError(nameof(CategoryViewModel.Name), "Ya existe una categoría con ese nombre.");
        }

        if (!ModelState.IsValid) return View("AddEdit", viewModel);

        await _categoryService.EditAsync(viewModel);
        TempData["SuccessMessage"] = "Categoría editada correctamente.";
        return RedirectToAction("Index");
    }

    [AcceptVerbs("Get", "Post")]
    public async Task<IActionResult> ValidateName(string name, int categoryId)
    {
        var exists = await _categoryService.ExistsByNameAsync(name, categoryId == 0 ? null : categoryId);
        return Json(!exists);
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
