using eCommerceMotoRepuestos.Models;
using eCommerceMotoRepuestos.Services;
using eCommerceMotoRepuestos.Utilities;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace eCommerceMotoRepuestos.Controllers
{
    public class HomeController(
        CategoryService _categoryService,
        ProductService _productService,
        OrderService _orderService
        ) : Controller
    {

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllActiveAsync();
            var products = await _productService.GetCatalogAsync();
            var catalog = new CatalogViewModel { Categories = categories, Products = products };
            return View(catalog);
        }


        public async Task<IActionResult> FilterByCategory(int id, string name)
        {
            var categories = await _categoryService.GetAllActiveAsync();
            var products = await _productService.GetCatalogAsync(categoryId: id);
            var catalog = new CatalogViewModel { Categories = categories, Products = products, FilterBy = name };
            return View("Index", catalog);
        }

        [HttpPost]
        public async Task<IActionResult> FilterBySearch(string value)
        {
            var categories = await _categoryService.GetAllActiveAsync();
            var products = await _productService.GetCatalogAsync(search: value);
            var catalog = new CatalogViewModel { Categories = categories, Products = products, FilterBy = $"Resultados para: {value}" };
            return View("Index", catalog);
        }


        public async Task<IActionResult> ProductDetail(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> AddItemToCart(int productId, int quantity)
        {
            var product = await _productService.GetByIdAsync(productId);
            if (quantity > product.Stock)
            {
                ViewBag.message = "En este momento no está disponible en stock la cantidad de unidades indicada.";
                ViewBag.alertType = "danger";
                return View("ProductDetail", product);
            }
            var cart = GetCart();
            var cartItem = cart.Find(x => x.ProductId == productId);

            if (cartItem is null)
            {
                cart.Add(new CartItemViewModel
                {
                    ProductId = productId,
                    ImageName = product.ImageName,
                    Name = product.Name,
                    Price = product.Price,
                    Quantity = quantity
                });
            }
            else
            {
                cartItem.Quantity += quantity;
            }

            HttpContext.Session.Set("Cart", cart);
            ViewBag.message = "Producto agregado al carrito";
            ViewBag.alertType = "success";
            return View("ProductDetail", product);
        }


        public IActionResult ViewCart()
        {
            var cart = GetCart();
            return View(cart);
        }


        public IActionResult RemoveItemToCart(int productId)
        {
            var cart = GetCart();
            var cartItem = cart.Find(x => x.ProductId == productId);
            cart.Remove(cartItem!);
            HttpContext.Session.Set("Cart", cart);
            return View("ViewCart", cart);
        }


        [HttpPost]
        public async Task<IActionResult> PayNow()
        {
            var cart = GetCart();
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            await _orderService.AddAsync(cart, int.Parse(userId));
            HttpContext.Session.Remove("Cart");
            return View("SaleCompleted");
        }

        public IActionResult SaleCompleted()
        {
            return View();
        }

        public List<CartItemViewModel> GetCart()
        {
            return HttpContext.Session.Get<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();
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

        public IActionResult TestError(int code = 404)
        {
            HttpContext.Response.StatusCode = code;
            return View("Error", new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                StatusCode = code
            });
        }
    }
}
