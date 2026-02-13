using eCommerceMotoRepuestos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace eCommerceMotoRepuestos.Controllers;

[Authorize]
public class UserController(OrderService _orderService) : Controller
{
    public async Task<IActionResult> MyOrders()
    {

        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var ordersvm = await _orderService.GetAllByUserAsync(int.Parse(userId));
        return View(ordersvm);
    }
}
