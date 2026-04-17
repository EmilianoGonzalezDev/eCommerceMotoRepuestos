using eCommerceMotoRepuestos.Enums;
using eCommerceMotoRepuestos.Models;
using eCommerceMotoRepuestos.Repositories;

namespace eCommerceMotoRepuestos.Services;

public class OrderService(OrderRepository _orderRepository)
{
    public async Task<List<OrderViewModel>> GetAllAsync()
    {
        var orders = await _orderRepository.GetAllWithDetailAsync();

        var ordersVM = orders.Select(x => new OrderViewModel
        {
            OrderDate = x.OrderDate,
            OrderId = x.OrderId,
            CustomerName = x.User?.FullName,
            TotalAmount = x.TotalAmount,
            PaymentType = x.PaymentType,
            Status = x.Status,
            OrderItems = x.OrderItems.Select(x => new OrderItemViewModel
            {
                ProductId = x.ProductId,
                ProductName = x.Product.Name,
                Quantity = x.Quantity,
                Price = x.Price
            }).ToList()
        }).ToList();

        return ordersVM;
    }

    public async Task<bool> UpdateStatusAsync(int orderId, OrderStatus status)
    {
        return await _orderRepository.UpdateStatusAsync(orderId, status);
    }

}
