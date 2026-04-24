using eCommerceMotoRepuestos.Entities;
using eCommerceMotoRepuestos.Models;
using eCommerceMotoRepuestos.Repositories;

namespace eCommerceMotoRepuestos.Services;

public class CartService(CartRepository _cartRepository)
{
    public async Task<List<CartItemViewModel>> GetAllByUserAsync(int userId)
    {
        var items = await _cartRepository.GetAllByUserWithProductAsync(userId);

        return items.Select(x => new CartItemViewModel
        {
            ProductId = x.ProductId,
            Name = x.Product?.Name ?? string.Empty,
            ImageName = x.Product?.ImageName ?? "default.png",
            Price = x.Product?.Price ?? 0,
            Quantity = x.Quantity,
            Stock = x.Product?.Stock ?? 0,
            IsActive = x.Product?.IsActive ?? false
        }).ToList();
    }

    public async Task<int> GetDistinctItemsCountAsync(int userId)
    {
        return await _cartRepository.GetDistinctItemsCountByUserAsync(userId);
    }

    public async Task<int> GetQuantityByUserAndProductAsync(int userId, int productId)
    {
        var item = await _cartRepository.GetByUserAndProductAsync(userId, productId);
        return item?.Quantity ?? 0;
    }

    public async Task<CartItem?> GetByUserAndProductAsync(int userId, int productId)
    {
        return await _cartRepository.GetByUserAndProductAsync(userId, productId);
    }

    public async Task AddOrIncrementAsync(int userId, ProductViewModel product, int quantity)
    {
        var existing = await _cartRepository.GetByUserAndProductAsync(userId, product.ProductId);

        if (existing is null)
        {
            await _cartRepository.AddAsync(new CartItem
            {
                UserId = userId,
                ProductId = product.ProductId,
                Quantity = quantity
            });
            return;
        }

        existing.Quantity += quantity;
        await _cartRepository.EditAsync(existing);
    }

    public async Task UpdateQuantityAsync(int userId, int productId, int quantity)
    {
        var existing = await _cartRepository.GetByUserAndProductAsync(userId, productId);
        if (existing is null) return;

        existing.Quantity = quantity;
        await _cartRepository.EditAsync(existing);
    }

    public async Task RemoveAsync(int userId, int productId)
    {
        var existing = await _cartRepository.GetByUserAndProductAsync(userId, productId);
        if (existing is null) return;

        await _cartRepository.DeleteAsync(existing);
    }

    public async Task ClearByUserAsync(int userId)
    {
        await _cartRepository.DeleteByUserAsync(userId);
    }
}
