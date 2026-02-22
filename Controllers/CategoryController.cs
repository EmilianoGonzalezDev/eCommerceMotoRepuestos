using eCommerceMotoRepuestos.Entities;
using eCommerceMotoRepuestos.Models;
using eCommerceMotoRepuestos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        var categoryViewModel = _categoryService.GetAddViewModel();
        return View("AddEdit", categoryViewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(CategoryViewModel viewModel)
    {
        ViewBag.message = null;

        if (!ModelState.IsValid) return View("AddEdit", viewModel);
        await _categoryService.AddAsync(viewModel);
        ModelState.Clear();
        ViewBag.message = "Categoría creada";
        return View("AddEdit", _categoryService.GetAddViewModel());
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var categoryViewModel = await _categoryService.GetEditViewModelAsync(id);
        if (categoryViewModel == null) return NotFound();
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
        await _categoryService.DeleteAsync(id);
        return RedirectToAction("Index");
    }

}
