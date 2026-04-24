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
            foreach (var detail in order.OrderItems)
            {
                var product = await _dbContext.Product.FindAsync(detail.ProductId);

                if (product is null)
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

    public async Task<IEnumerable<Order>> GetAllWithDetailAsync(int userId)
    {
        var orders = await _dbContext.Order
                            .Where(x => x.UserId == userId)
                            .Include(x => x.OrderItems)
                            .ThenInclude(x => x.Product)
                            .OrderByDescending(x => x.OrderDate)
                            .ToListAsync();
        return orders;
    }

    public async Task<IEnumerable<Order>> GetAllWithDetailAsync()
    {
        var orders = await _dbContext.Order
                            .Include(x => x.User)
                            .Include(x => x.OrderItems)
                            .ThenInclude(x => x.Product)
                            .OrderByDescending(x => x.OrderDate)
                            .ToListAsync();
        return orders;
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
