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
    public async Task<IActionResult> Index()
    {
        var categories = await _categoryService.GetAllAsync();
        return View(categories);
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
        TempData["SuccessMessage"] = "Categoría creada.";
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
        ViewBag.message = null;
        if (!ModelState.IsValid) return View("AddEdit", viewModel);

        await _categoryService.EditAsync(viewModel);
        ViewBag.message = "Categoría editada";
        return View("AddEdit", viewModel);
    }

    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _categoryService.DeleteAsync(id);
            TempData["SuccessMessage"] = "Categoría eliminada.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch
        {
            TempData["ErrorMessage"] = "No se pudo eliminar la categoría.";
        }

        return RedirectToAction("Index");
    }

}
