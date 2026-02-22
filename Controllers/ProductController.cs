using eCommerceMotoRepuestos.Models;
using eCommerceMotoRepuestos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eCommerceMotoRepuestos.Controllers;

[Authorize(Roles = "Admin")]
public class ProductController(ProductService _productService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var products = await _productService.GetAllAsync();
        return View(products);
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
        ViewBag.message = null;
        ModelState.Remove("Categories");
        ModelState.Remove("Category.Name");
        if (!ModelState.IsValid)
        {
            entityVM.Category ??= new CategoryViewModel();
            await _productService.PopulateCategoriesAsync(entityVM);
            return View("AddEdit", entityVM);
        }

        await _productService.AddAsync(entityVM);
        ModelState.Clear();
        entityVM = await _productService.GetAddViewModelAsync();
        ViewBag.message = "Producto creado";
        return View("AddEdit", entityVM);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var productVM = await _productService.GetEditViewModelAsync(id);
        if (productVM == null) return NotFound();
        return View("AddEdit", productVM);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProductViewModel entityVM)
    {
        ViewBag.message = null;
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
        ViewBag.message = "Producto editado";
        return View("AddEdit", entityVM);
    }


    public async Task<IActionResult> Delete(int id)
    {
        await _productService.DeleteAsync(id);
        return RedirectToAction("Index");
    }
}
