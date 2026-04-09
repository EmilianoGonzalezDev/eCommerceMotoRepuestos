using eCommerceMotoRepuestos.Models;
using eCommerceMotoRepuestos.Services;
using eCommerceMotoRepuestos.Utilities;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace eCommerceMotoRepuestos.Controllers
{
    public class CartController(
        ProductService _productService,
        OrderService _orderService
        ) : Controller
    {
        private const int CartPageSize = 10;

        [HttpPost]
        public async Task<IActionResult> AddItemToCart(int productId, int quantity)
        {
            var product = await _productService.GetByIdAsync(productId);
            if (quantity > product.Stock)
            {
                ViewBag.message = "En este momento no está disponible en stock la cantidad de unidades indicada.";
                ViewBag.alertType = "danger";
                return View("~/Views/Home/ProductDetail.cshtml", product);
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
            return View("~/Views/Home/ProductDetail.cshtml", product);
        }

        public IActionResult ViewCart(int page = 1)
        {
            var cart = GetCart();
            var pagedCart = PagedResult<CartItemViewModel>.Create(cart, page, CartPageSize);
            return View("~/Views/Home/ViewCart.cshtml", pagedCart);
        }

        public IActionResult RemoveItemToCart(int productId, int page = 1)
        {
            var cart = GetCart();
            var cartItem = cart.Find(x => x.ProductId == productId);
            if (cartItem is not null)
            {
                cart.Remove(cartItem);
                HttpContext.Session.Set("Cart", cart);
            }

            return RedirectToAction("ViewCart", new { page });
        }

        [HttpPost]
        public async Task<IActionResult> PayNow()
        {
            var cart = GetCart();
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            await _orderService.AddAsync(cart, int.Parse(userId));
            HttpContext.Session.Remove("Cart");
            return View("~/Views/Home/SaleCompleted.cshtml");
        }

        public IActionResult SaleCompleted()
        {
            return View("~/Views/Home/SaleCompleted.cshtml");
        }

        private List<CartItemViewModel> GetCart()
        {
            return HttpContext.Session.Get<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();
        }
    }
}
