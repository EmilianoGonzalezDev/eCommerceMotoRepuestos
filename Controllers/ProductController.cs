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
    public async Task<IActionResult> AddEdit(int id)
    {
        var productVM = await _productService.GetByIdAsync(id);
        return View(productVM);
    }

    [HttpPost]
    public async Task<IActionResult> AddEdit(ProductViewModel entityVM)
    {
        ViewBag.message = null;
        ModelState.Remove("Categories");
        ModelState.Remove("Category.Name");
        if (!ModelState.IsValid) return View(entityVM);

        if (entityVM.ProductId == 0)
        {
            await _productService.AddAsync(entityVM);
            ModelState.Clear();
            entityVM = new ProductViewModel();
            ViewBag.message = "Producto creado";
        }
        else
        {
            await _productService.EditAsync(entityVM);
            ViewBag.message = "Producto editado";
        }
        return View(entityVM);
    }


    public async Task<IActionResult> Delete(int id)
    {
        await _productService.DeleteAsync(id);
        return RedirectToAction("Index");
    }
}
