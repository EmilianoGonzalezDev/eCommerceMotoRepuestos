using eCommerceMotoRepuestos.Models;
using eCommerceMotoRepuestos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace eCommerceMotoRepuestos.Controllers;

[Authorize]
public class UserController(OrderService _orderService) : Controller
{
    private const int OrdersPageSize = 10;

    public async Task<IActionResult> MyOrders(int page = 1)
    {

        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var ordersvm = await _orderService.GetAllByUserAsync(int.Parse(userId));
        var pagedOrders = PagedResult<OrderViewModel>.Create(ordersvm, page, OrdersPageSize);
        return View(pagedOrders);
    }
}

