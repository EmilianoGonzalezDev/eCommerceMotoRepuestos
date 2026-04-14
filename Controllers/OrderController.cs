using eCommerceMotoRepuestos.Enums;
using eCommerceMotoRepuestos.Models;
using eCommerceMotoRepuestos.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eCommerceMotoRepuestos.Controllers;

[Authorize(Roles = "Admin")]
[Route("Orders")]
public class OrderController(OrderService _orderService) : Controller
{
    private static readonly int[] PageSizes = [5, 10, 15, 20, 50, 100];
    private const int DefaultOrdersPageSize = 10;
    private static readonly OrderStatus[] DefaultOrderFilters = [OrderStatus.Pending, OrderStatus.Prepared];

    private static int NormalizePageSize(int? pageSize, int defaultSize)
    {
        if (pageSize is null) return defaultSize;
        return Array.IndexOf(PageSizes, pageSize.Value) >= 0 ? pageSize.Value : defaultSize;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(int page = 1, int pageSize = DefaultOrdersPageSize, List<OrderStatus>? selectedStatuses = null, bool filtersSubmitted = false)
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

    [HttpPost("UpdateStatus")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(
        int orderId,
        OrderStatus status,
        int page = 1,
        int pageSize = DefaultOrdersPageSize,
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
