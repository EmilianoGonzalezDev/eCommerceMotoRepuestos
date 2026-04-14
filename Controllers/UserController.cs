using eCommerceMotoRepuestos.Models;
using eCommerceMotoRepuestos.Services;
using eCommerceMotoRepuestos.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace eCommerceMotoRepuestos.Controllers;

[Authorize]
public class UserController(OrderService _orderService) : Controller
{
    private static readonly int[] PageSizes = [5, 10, 15, 20, 50, 100];
    private const int DefaultOrdersPageSize = 10;
    private static readonly OrderStatus[] DefaultOrderFilters = [OrderStatus.Pending, OrderStatus.Prepared];

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
    public async Task<IActionResult> Orders(int page = 1, int pageSize = DefaultOrdersPageSize, List<OrderStatus>? selectedStatuses = null, bool filtersSubmitted = false)
    {
        var size = NormalizePageSize(pageSize, DefaultOrdersPageSize);
        var effectiveStatuses = filtersSubmitted
            ? selectedStatuses ?? []
            : DefaultOrderFilters.ToList();

        var ordersvm = await _orderService.GetAllAsync();
        ordersvm = ordersvm
            .Where(order => effectiveStatuses.Contains(order.Status))
            .ToList();

        var pagedOrders = PagedResult<OrderViewModel>.Create(ordersvm, page, size);
        ViewBag.SelectedStatuses = effectiveStatuses.Select(status => (int)status).ToHashSet();
        return View(pagedOrders);
    }
}


