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
        })
        .OrderBy(x => x.Name)
        .ToList();
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
        await AddOrIncrementAsync(userId, product, quantity, existing);
    }

    public async Task AddOrIncrementAsync(int userId, ProductViewModel product, int quantity, CartItem? existing)
    {
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

        await UpdateQuantityAsync(existing, quantity);
    }

    public async Task UpdateQuantityAsync(CartItem existing, int quantity)
    {
        existing.Quantity = quantity;
        await _cartRepository.EditAsync(existing);
    }

    /// <summary>
    /// Merges items (e.g. from a guest session cart) into the user's cart using a fixed number
    /// of queries and a single save. Quantities are capped to the available stock.
    /// </summary>
    public async Task MergeAsync(int userId, IEnumerable<ProductViewModel> products, IReadOnlyDictionary<int, int> quantitiesByProduct)
    {
        var existingItems = (await _cartRepository.GetByUserAndProductsAsync(userId, quantitiesByProduct.Keys))
            .ToDictionary(x => x.ProductId);
        var newItems = new List<CartItem>();

        foreach (var product in products)
        {
            existingItems.TryGetValue(product.ProductId, out var existing);
            var currentQuantity = existing?.Quantity ?? 0;
            var requested = quantitiesByProduct[product.ProductId];
            var adjusted = currentQuantity + requested > product.Stock
                ? product.Stock - currentQuantity
                : requested;

            if (adjusted <= 0) continue;

            if (existing is null)
            {
                newItems.Add(new CartItem { UserId = userId, ProductId = product.ProductId, Quantity = adjusted });
            }
            else
            {
                existing.Quantity += adjusted;
            }
        }

        await _cartRepository.AddRangeAndSaveAsync(newItems);
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
