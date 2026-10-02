using eCommerceMotoRepuestos.Context;
using eCommerceMotoRepuestos.Entities;
using eCommerceMotoRepuestos.Enums;
using Microsoft.EntityFrameworkCore;

namespace eCommerceMotoRepuestos.Repositories;

public class OrderRepository : GenericRepository<Order>
{
    private readonly AppDbContext _dbContext;
    public OrderRepository(AppDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }
    public override async Task AddAsync(Order order)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            var productIds = order.OrderItems.Select(x => x.ProductId).Distinct().ToList();
            var products = await _dbContext.Product
                .Where(x => productIds.Contains(x.ProductId))
                .ToDictionaryAsync(x => x.ProductId);

            foreach (var detail in order.OrderItems)
            {
                if (!products.TryGetValue(detail.ProductId, out var product))
                {
                    throw new KeyNotFoundException();
                }

                if (product.Stock < detail.Quantity)
                { 
                    throw new InvalidOperationException();
                }

                product.Stock -= detail.Quantity;
            }

            await _dbContext.Order.AddAsync(order);
            await _dbContext.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

    }

    public async Task<bool> UpdateStatusAsync(int orderId, OrderStatus status)
    {
        var order = await _dbContext.Order.FindAsync(orderId);
        if (order is null) return false;

        order.Status = status;
        await _dbContext.SaveChangesAsync();
        return true;
    }
}
