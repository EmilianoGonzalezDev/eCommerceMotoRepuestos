using eCommerceMotoRepuestos.Entities;
using eCommerceMotoRepuestos.Enums;
using eCommerceMotoRepuestos.Models;
using eCommerceMotoRepuestos.Repositories;

namespace eCommerceMotoRepuestos.Services;

public class OrderService(OrderRepository _orderRepository)
{

    public async Task AddAsync(List<CartItemViewModel> cartItemVM, int userId, PaymentType paymentType)
    {

        Order order = new Order()
        {
            OrderDate = DateTime.Now,
            UserId = userId,
            TotalAmount = cartItemVM.Sum(x => x.Price * x.Quantity),
            PaymentType = paymentType,
            Status = OrderStatus.Pending,
            OrderItems = cartItemVM.Select(x => new OrderItem
            {
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                Price = x.Price
            }).ToList()
        };

        await _orderRepository.AddAsync(order);

    }

    public async Task<List<OrderViewModel>> GetAllByUserAsync(int userId)
    {
        var orders = await _orderRepository.GetAllWithDetailAsync(userId);

        var ordersVM = orders.Select(x => new OrderViewModel
        {
            OrderDate = x.OrderDate,
            TotalAmount = x.TotalAmount,
            PaymentType = x.PaymentType,
            Status = x.Status,
            OrderItems = x.OrderItems.Select(x => new OrderItemViewModel
            {
                ProductName = x.Product.Name,
                Quantity = x.Quantity,
                Price = x.Price
            }).ToList()
        }).ToList();

        return ordersVM;
    }

    public async Task<List<OrderViewModel>> GetAllAsync()
    {
        var orders = await _orderRepository.GetAllWithDetailAsync();

        var ordersVM = orders.Select(x => new OrderViewModel
        {
            OrderDate = x.OrderDate,
            CustomerName = x.User?.FullName,
            TotalAmount = x.TotalAmount,
            PaymentType = x.PaymentType,
            Status = x.Status,
            OrderItems = x.OrderItems.Select(x => new OrderItemViewModel
            {
                ProductName = x.Product.Name,
                Quantity = x.Quantity,
                Price = x.Price
            }).ToList()
        }).ToList();

        return ordersVM;
    }

}
