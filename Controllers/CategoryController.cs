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
    public async Task<IActionResult> AddEdit(int id)
    {
        var categoryViewModel = await _categoryService.GetByIdAsync(id);
        return View(categoryViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> AddEdit(CategoryViewModel viewModel)
    {
        ViewBag.message = null;

        if (!ModelState.IsValid) return View(viewModel);

        if (viewModel.CategoryId == 0)
        {
            await _categoryService.AddAsync(viewModel);
            ModelState.Clear();
            viewModel = new CategoryViewModel();
            ViewBag.message = "Created category";
        }
        else
        {
            await _categoryService.EditAsync(viewModel);
            ViewBag.message = "edited category";
        }

        return View(viewModel);
    }

    public async Task<IActionResult> Delete(int id)
    {
        await _categoryService.DeleteAsync(id);
        return RedirectToAction("Index");
    }

}
