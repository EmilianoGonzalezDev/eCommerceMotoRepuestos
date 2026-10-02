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

        public async Task<IActionResult> Index(int page = 1)
        {
            var categories = await _categoryService.GetAllActiveAsync();
            var pagedProducts = await _productService.GetCatalogPagedAsync(page, PaginationSettings.CatalogPageSize);
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
            var pagedProducts = await _productService.GetCatalogPagedAsync(page, PaginationSettings.CatalogPageSize, categoryId: id);
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
            var searchValue = (value ?? string.Empty).Trim();
            var categories = await _categoryService.GetAllActiveAsync();
            var pagedProducts = await _productService.GetCatalogPagedAsync(page, PaginationSettings.CatalogPageSize, search: searchValue);
            var catalog = new CatalogViewModel
            {
                Categories = categories,
                Products = pagedProducts,
                FilterBy = $"Resultados para: {searchValue}",
                CategoryFilterId = 0,
                SearchValue = searchValue
            };
            return View("Index", catalog);
        }

        [HttpGet]
        public async Task<IActionResult> SearchSuggestions(string value)
        {
            var searchValue = (value ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(searchValue))
            {
                return Json(Array.Empty<object>());
            }

            var products = await _productService.GetSearchSuggestionsAsync(searchValue, limit: 10);
            var suggestions = products
                .Select(p => new
                {
                    p.ProductId,
                    p.Name,
                    p.Price,
                    PriceFormatted = p.Price.ToString("C")
                });

            return Json(suggestions);
        }


        public async Task<IActionResult> ProductDetail(int id, string? returnUrl = null)
        {
            var product = await _productService.GetByIdAsync(id);
            ViewBag.ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : null;
            return View(product);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? statusCode = null, string? errorMessage = null)
        {
            var status = statusCode ?? HttpContext.Response.StatusCode;
            HttpContext.Response.StatusCode = status;
            var errorViewModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                StatusCode = status,
                ErrorMessage = errorMessage
            };
            return View(errorViewModel);
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return View();
        }
    }
}

