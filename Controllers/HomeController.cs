using eCommerceMotoRepuestos.Models;
using eCommerceMotoRepuestos.Services;
using eCommerceMotoRepuestos.Utilities;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace eCommerceMotoRepuestos.Controllers
{
    public class HomeController(
        CategoryService _categoryService,
        ProductService _productService
        ) : Controller
    {
        private const int CatalogPageSize = 12;

        public async Task<IActionResult> Index(int page = 1)
        {
            var categories = await _categoryService.GetAllActiveAsync();
            var products = await _productService.GetCatalogAsync();
            var pagedProducts = PagedResult<ProductViewModel>.Create(products, page, CatalogPageSize);
            var catalog = new CatalogViewModel
            {
                Categories = categories,
                Products = pagedProducts,
                CategoryFilterId = 0,
                SearchValue = null
            };
            return View(catalog);
        }


        public async Task<IActionResult> FilterByCategory(int id, string name, int page = 1)
        {
            var categories = await _categoryService.GetAllActiveAsync();
            var products = await _productService.GetCatalogAsync(categoryId: id);
            var pagedProducts = PagedResult<ProductViewModel>.Create(products, page, CatalogPageSize);
            var catalog = new CatalogViewModel
            {
                Categories = categories,
                Products = pagedProducts,
                FilterBy = name,
                CategoryFilterId = id,
                SearchValue = null
            };
            return View("Index", catalog);
        }

        [HttpGet]
        public async Task<IActionResult> FilterBySearch(string value, int page = 1)
        {
            var categories = await _categoryService.GetAllActiveAsync();
            var products = await _productService.GetCatalogAsync(search: value);
            var pagedProducts = PagedResult<ProductViewModel>.Create(products, page, CatalogPageSize);
            var catalog = new CatalogViewModel
            {
                Categories = categories,
                Products = pagedProducts,
                FilterBy = $"Resultados para: {value}",
                CategoryFilterId = 0,
                SearchValue = value
            };
            return View("Index", catalog);
        }

        [HttpGet]
        public async Task<IActionResult> SearchSuggestions(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Json(Array.Empty<object>());
            }

            var products = await _productService.GetCatalogAsync(search: value.Trim());
            var suggestions = products
                .OrderByDescending(p => p.Name.Equals(value, StringComparison.OrdinalIgnoreCase))
                .ThenByDescending(p => p.Name.StartsWith(value, StringComparison.OrdinalIgnoreCase))
                .ThenBy(p => p.Name.Length)
                .ThenBy(p => p.Name)
                .Take(10)
                .Select(p => new
                {
                    p.ProductId,
                    p.Name,
                    p.Price,
                    PriceFormatted = p.Price.ToString("C")
                });

            return Json(suggestions);
        }


        public async Task<IActionResult> ProductDetail(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            return View(product);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? statusCode = null)
        {
            var status = statusCode ?? HttpContext.Response.StatusCode;
            var errorViewModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                StatusCode = status
            };
            return View(errorViewModel);
        }
    }
}

