using eCommerceMotoRepuestos.Entities;
using eCommerceMotoRepuestos.Models;
using eCommerceMotoRepuestos.Repositories;

namespace eCommerceMotoRepuestos.Services;

public class OrderService(OrderRepository _orderRepository)
{

    public async Task AddAsync(List<CartItemViewModel> cartItemVM, int userId)
    {

        Order order = new Order()
        {
            OrderDate = DateTime.Now,
            UserId = userId,
            TotalAmount = cartItemVM.Sum(x => x.Price * x.Quantity),
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
            OrderDate = x.OrderDate.ToString("dd/MM/yyyy"),
            TotalAmount = x.TotalAmount.ToString("C2"),
            OrderItems = x.OrderItems.Select(x => new OrderItemViewModel
            {
                ProductName = x.Product.Name,
                Quantity = x.Quantity,
                Price = x.Price.ToString("C2")
            }).ToList()
        }).ToList();

        return ordersVM;
    }

}
