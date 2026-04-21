using eCommerceMotoRepuestos.Models;
using eCommerceMotoRepuestos.Services;
using eCommerceMotoRepuestos.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace eCommerceMotoRepuestos.Controllers;

[Authorize]
public class UserController(UserOrderService _userOrderService) : Controller
{
    private static int NormalizePageSize(int? pageSize, int defaultSize)
    {
        if (pageSize is null) return defaultSize;
        return Array.IndexOf(PaginationSettings.PageSizes, pageSize.Value) >= 0 ? pageSize.Value : defaultSize;
    }

    public async Task<IActionResult> MyOrders(int page = 1, int pageSize = PaginationSettings.DefaultPageSize)
    {
        var size = NormalizePageSize(pageSize, PaginationSettings.DefaultPageSize);
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var ordersvm = await _userOrderService.GetAllByUserAsync(int.Parse(userId));
        var pagedOrders = PagedResult<OrderViewModel>.Create(ordersvm, page, size);
        return View(pagedOrders);
    }
}


