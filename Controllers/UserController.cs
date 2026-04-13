using eCommerceMotoRepuestos.Models;
using eCommerceMotoRepuestos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace eCommerceMotoRepuestos.Controllers;

[Authorize]
public class UserController(OrderService _orderService) : Controller
{
    private static readonly int[] PageSizes = [5, 10, 15, 20, 50, 100];
    private const int DefaultOrdersPageSize = 10;

    private static int NormalizePageSize(int? pageSize, int defaultSize)
    {
        if (pageSize is null) return defaultSize;
        return Array.IndexOf(PageSizes, pageSize.Value) >= 0 ? pageSize.Value : defaultSize;
    }

    public async Task<IActionResult> MyOrders(int page = 1, int pageSize = DefaultOrdersPageSize)
    {
        var size = NormalizePageSize(pageSize, DefaultOrdersPageSize);
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var ordersvm = await _orderService.GetAllByUserAsync(int.Parse(userId));
        var pagedOrders = PagedResult<OrderViewModel>.Create(ordersvm, page, size);
        return View(pagedOrders);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Orders(int page = 1, int pageSize = DefaultOrdersPageSize)
    {
        var size = NormalizePageSize(pageSize, DefaultOrdersPageSize);
        var ordersvm = await _orderService.GetAllAsync();
        var pagedOrders = PagedResult<OrderViewModel>.Create(ordersvm, page, size);
        return View(pagedOrders);
    }
}


