using eCommerceMotoRepuestos.Context;
using eCommerceMotoRepuestos.Entities;
using Microsoft.EntityFrameworkCore;

namespace eCommerceMotoRepuestos.Repositories;

public class CartRepository(AppDbContext _dbContext)
{
    public async Task<List<CartItem>> GetAllByUserWithProductAsync(int userId)
    {
        return await _dbContext.CartItem
            .Where(x => x.UserId == userId)
            .Include(x => x.Product)
            .ToListAsync();
    }

    public async Task<CartItem?> GetByUserAndProductAsync(int userId, int productId)
    {
        return await _dbContext.CartItem
            .FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId);
    }

    public async Task<int> GetDistinctItemsCountByUserAsync(int userId)
    {
        return await _dbContext.CartItem.CountAsync(x => x.UserId == userId);
    }

    public async Task AddAsync(CartItem item)
    {
        await _dbContext.CartItem.AddAsync(item);
        await _dbContext.SaveChangesAsync();
    }

    public async Task EditAsync(CartItem item)
    {
        _dbContext.CartItem.Update(item);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(CartItem item)
    {
        _dbContext.CartItem.Remove(item);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteByUserAsync(int userId)
    {
        var items = await _dbContext.CartItem
            .Where(x => x.UserId == userId)
            .ToListAsync();

        if (items.Count == 0) return;

        _dbContext.CartItem.RemoveRange(items);
        await _dbContext.SaveChangesAsync();
    }
}
