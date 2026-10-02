using eCommerceMotoRepuestos.Entities;
using eCommerceMotoRepuestos.Enums;
using eCommerceMotoRepuestos.Models;
using eCommerceMotoRepuestos.Repositories;
using Microsoft.EntityFrameworkCore;

namespace eCommerceMotoRepuestos.Services;

public class OrderService(OrderRepository _orderRepository)
{
    public async Task<PagedResult<OrderViewModel>> GetPagedAsync(IReadOnlyCollection<OrderStatus> statuses, int page, int pageSize)
    {
        var query = _orderRepository.Query()
            .Where(o => statuses.Contains(o.Status))
            .OrderByDescending(o => o.OrderDate)
            .Select(o => new OrderViewModel
            {
                OrderDate = o.OrderDate,
                OrderId = o.OrderId,
                CustomerName = o.User!.FullName,
                TotalAmount = o.TotalAmount,
                PaymentType = o.PaymentType,
                Status = o.Status,
                OrderItems = o.OrderItems.Select(oi => new OrderItemViewModel
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    Quantity = oi.Quantity,
                    Price = oi.Price
                }).ToList()
            });

        return await PagedResult<OrderViewModel>.CreateAsync(query, page, pageSize);
    }

    public async Task<PagedResult<OrderViewModel>> GetPagedByUserAsync(int userId, int page, int pageSize)
    {
        var query = _orderRepository.Query()
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .Select(o => new OrderViewModel
            {
                OrderDate = o.OrderDate,
                OrderId = o.OrderId,
                TotalAmount = o.TotalAmount,
                PaymentType = o.PaymentType,
                Status = o.Status,
                OrderItems = o.OrderItems.Select(oi => new OrderItemViewModel
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    Quantity = oi.Quantity,
                    Price = oi.Price
                }).ToList()
            });

        return await PagedResult<OrderViewModel>.CreateAsync(query, page, pageSize);
    }

    public async Task<bool> UpdateStatusAsync(int orderId, OrderStatus status)
    {
        return await _orderRepository.UpdateStatusAsync(orderId, status);
    }

    public async Task<OrderResult> AddAsync(List<CartItemViewModel> cartItemVM, int userId, PaymentType paymentType)
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

        try
        {
            await _orderRepository.AddAsync(order);
            return OrderResult.Success;
        }
        catch (KeyNotFoundException)
        {
            return OrderResult.ProductNotFound;
        }
        catch (InvalidOperationException)
        {
            return OrderResult.InsufficientStock;
        }

    }

}
