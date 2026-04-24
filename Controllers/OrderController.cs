using eCommerceMotoRepuestos.Enums;
using eCommerceMotoRepuestos.Models;
using eCommerceMotoRepuestos.Services;
using eCommerceMotoRepuestos.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace eCommerceMotoRepuestos.Controllers;

[Authorize]
[Route("Orders")]
public class OrderController(OrderService _orderService) : Controller
{
    private static readonly OrderStatus[] DefaultOrderFilters = [OrderStatus.Pending, OrderStatus.Prepared];

    private static int NormalizePageSize(int? pageSize, int defaultSize)
    {
        if (pageSize is null) return defaultSize;
        return Array.IndexOf(PaginationSettings.PageSizes, pageSize.Value) >= 0 ? pageSize.Value : defaultSize;
    }

    [HttpGet("MyOrders")]
    public async Task<IActionResult> MyOrders(int page = 1, int pageSize = PaginationSettings.DefaultPageSize)
    {
        var size = NormalizePageSize(pageSize, PaginationSettings.DefaultPageSize);
        var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var ordersvm = await _orderService.GetAllByUserAsync(int.Parse(userId));
        var pagedOrders = PagedResult<OrderViewModel>.Create(ordersvm, page, size);
        return View(pagedOrders);
    }

    [HttpGet("")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index(int page = 1, int pageSize = PaginationSettings.DefaultPageSize, List<OrderStatus>? selectedStatuses = null, bool filtersSubmitted = false)
    {
        var size = NormalizePageSize(pageSize, PaginationSettings.DefaultPageSize);
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

    [HttpPost("UpdateStatus")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(
        int orderId,
        OrderStatus status,
        int page = 1,
        int pageSize = PaginationSettings.DefaultPageSize,
        List<OrderStatus>? selectedStatuses = null,
        bool filtersSubmitted = true)
    {
        await _orderService.UpdateStatusAsync(orderId, status);

        return RedirectToAction(nameof(Index), new
        {
            page,
            pageSize,
            selectedStatuses,
            filtersSubmitted
        });
    }
}
