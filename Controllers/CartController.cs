using eCommerceMotoRepuestos.Models;
using eCommerceMotoRepuestos.Services;
using eCommerceMotoRepuestos.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace eCommerceMotoRepuestos.Controllers
{
    public class CartController(
        ProductService _productService,
        CartService _cartService,
        OrderService _orderService
        ) : Controller
    {
        private const int CartPageSize = 10;

        [HttpPost]
        public async Task<IActionResult> AddItemToCart(int productId, int quantity)
        {
            var product = await _productService.GetByIdAsync(productId);
            var requestedQuantity = quantity < 1 ? 1 : quantity;
            var userId = GetAuthenticatedUserId();

            if (userId is not null)
            {
                var currentQuantity = await _cartService.GetQuantityByUserAndProductAsync(userId.Value, productId);

                if (requestedQuantity + currentQuantity > product.Stock)
                {
                    ViewBag.message = "En este momento no está disponible en stock la cantidad de unidades indicada.";
                    ViewBag.alertType = "danger";
                    return View("~/Views/Home/ProductDetail.cshtml", product);
                }

                await _cartService.AddOrIncrementAsync(userId.Value, product, requestedQuantity);
            }
            else
            {
                var cart = GetSessionCart();
                var cartItem = cart.Find(x => x.ProductId == productId);
                var currentQuantity = cartItem?.Quantity ?? 0;

                if (requestedQuantity + currentQuantity > product.Stock)
                {
                    ViewBag.message = "En este momento no está disponible en stock la cantidad de unidades indicada.";
                    ViewBag.alertType = "danger";
                    return View("~/Views/Home/ProductDetail.cshtml", product);
                }

                if (cartItem is null)
                {
                    cart.Add(new CartItemViewModel
                    {
                        ProductId = productId,
                        ImageName = product.ImageName ?? "default.png",
                        Name = product.Name,
                        Price = product.Price,
                        Quantity = requestedQuantity
                    });
                }
                else
                {
                    cartItem.Quantity += requestedQuantity;
                }

                HttpContext.Session.Set("Cart", cart);
            }

            ViewBag.message = "Producto agregado al carrito";
            ViewBag.alertType = "success";
            return View("~/Views/Home/ProductDetail.cshtml", product);
        }

        public async Task<IActionResult> ViewCart(int page = 1)
        {
            var cart = await GetCartAsync();
            ViewBag.OrderTotal = cart.Sum(x => x.Price * x.Quantity);
            var pagedCart = PagedResult<CartItemViewModel>.Create(cart, page, CartPageSize);
            return View(pagedCart);
        }

        public async Task<IActionResult> RemoveItemToCart(int productId, int page = 1)
        {
            var userId = GetAuthenticatedUserId();

            if (userId is not null)
            {
                await _cartService.RemoveAsync(userId.Value, productId);
            }
            else
            {
                var cart = GetSessionCart();
                var cartItem = cart.Find(x => x.ProductId == productId);
                if (cartItem is not null)
                {
                    cart.Remove(cartItem);
                    HttpContext.Session.Set("Cart", cart);
                }
            }

            return RedirectToAction("ViewCart", new { page });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateItemQuantity(int productId, int quantity, int page = 1)
        {
            var userId = GetAuthenticatedUserId();
            var adjustedQuantity = quantity < 1 ? 1 : quantity;

            if (adjustedQuantity != quantity)
            {
                TempData["CartMessage"] = "La cantidad mínima permitida es 1.";
                TempData["CartAlertType"] = "warning";
            }

            var product = await _productService.GetByIdAsync(productId);
            if (product.Stock < 1)
            {
                TempData["CartMessage"] = "No hay stock disponible para este producto en este momento.";
                TempData["CartAlertType"] = "danger";
                return RedirectToAction("ViewCart", new { page });
            }

            if (adjustedQuantity > product.Stock)
            {
                adjustedQuantity = product.Stock;
                TempData["CartMessage"] = $"Se ajustó la cantidad al stock disponible ({product.Stock}).";
                TempData["CartAlertType"] = "warning";
            }

            if (userId is not null)
            {
                var cartItem = await _cartService.GetByUserAndProductAsync(userId.Value, productId);
                if (cartItem is null)
                {
                    TempData["CartMessage"] = "No se encontró el producto en el carrito.";
                    TempData["CartAlertType"] = "warning";
                    return RedirectToAction("ViewCart", new { page });
                }

                await _cartService.UpdateQuantityAsync(userId.Value, productId, adjustedQuantity);
            }
            else
            {
                var cart = GetSessionCart();
                var cartItem = cart.Find(x => x.ProductId == productId);
                if (cartItem is null)
                {
                    TempData["CartMessage"] = "No se encontró el producto en el carrito.";
                    TempData["CartAlertType"] = "warning";
                    return RedirectToAction("ViewCart", new { page });
                }

                cartItem.Quantity = adjustedQuantity;
                HttpContext.Session.Set("Cart", cart);
            }

            return RedirectToAction("ViewCart", new { page });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CompletePurchase()
        {
            var userId = GetAuthenticatedUserId();
            if (userId is null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = await _cartService.GetAllByUserAsync(userId.Value);
            if (cart.Count == 0)
            {
                TempData["CartMessage"] = "No hay productos en el carrito para finalizar la compra.";
                TempData["CartAlertType"] = "warning";
                return RedirectToAction("ViewCart");
            }

            await _orderService.AddAsync(cart, userId.Value);
            await _cartService.ClearByUserAsync(userId.Value);
            HttpContext.Session.Remove("Cart");
            return View("SaleCompleted");
        }

        public IActionResult SaleCompleted()
        {
            return View();
        }

        private int? GetAuthenticatedUserId()
        {
            if (HttpContext.User.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            var claim = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claim, out var userId))
            {
                return null;
            }

            return userId;
        }

        private async Task<List<CartItemViewModel>> GetCartAsync()
        {
            var userId = GetAuthenticatedUserId();
            if (userId is not null)
            {
                return await _cartService.GetAllByUserAsync(userId.Value);
            }

            return GetSessionCart();
        }

        private List<CartItemViewModel> GetSessionCart()
        {
            return HttpContext.Session.Get<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();
        }
    }
}
