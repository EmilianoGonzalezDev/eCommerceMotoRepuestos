using eCommerceMotoRepuestos.Entities;
using eCommerceMotoRepuestos.Models;
using eCommerceMotoRepuestos.Repositories;

namespace eCommerceMotoRepuestos.Services;

public class CategoryService(
    GenericRepository<Category> _categoryRepository,
    GenericRepository<Product> _productRepository)
{
    private static string NormalizeName(string name)
    {
        return name.Trim().ToUpperInvariant();
    }

    private static string SanitizeName(string? name)
    {
        return (name ?? string.Empty).Trim();
    }

    public async Task<IEnumerable<CategoryViewModel>> GetAllAsync()
    {
        var categoriesVieModel = await _categoryRepository.GetAllProjectedAsync(item =>
        new CategoryViewModel
        {
            CategoryId = item.CategoryId,
            Name = item.Name,
            IsActive = item.IsActive
        });

        return categoriesVieModel.OrderBy(c => c.Name).ToList();
    }

    public async Task<IEnumerable<CategoryViewModel>> GetAllActiveAsync()
    {
        return await _categoryRepository.GetAllProjectedAsync(item =>
        new CategoryViewModel
        {
            CategoryId = item.CategoryId,
            Name = item.Name,
            IsActive = item.IsActive
        },
        conditions: [c => c.IsActive]);
    }

    public async Task AddAsync(CategoryViewModel viewModel)
    {
        var entity = new Category
        {
            Name = SanitizeName(viewModel.Name),
            IsActive = true
        };

        await _categoryRepository.AddAsync(entity);
    }

    public async Task<CategoryViewModel?> GetEditViewModelAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);

        if (category is null) return null;

        var categoryViewModel = new CategoryViewModel
        {
            Name = category.Name,
            CategoryId = category.CategoryId,
            IsActive = category.IsActive
        };

        return categoryViewModel;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _categoryRepository.AnyAsync([c => c.CategoryId == id]);
    }

    public async Task EditAsync(CategoryViewModel viewModel)
    {
        var category = await _categoryRepository.GetByIdAsync(viewModel.CategoryId);
        if (category is null) return;

        category.Name = SanitizeName(viewModel.Name);
        await _categoryRepository.EditAsync(category);
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludedCategoryId = null)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;

        var normalizedName = NormalizeName(name);
        return await _categoryRepository.AnyAsync(
            [
                c => c.Name.ToUpper() == normalizedName,
                c => !excludedCategoryId.HasValue || c.CategoryId != excludedCategoryId.Value
            ]);
    }

    public async Task<bool> ToggleActiveAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category is null) throw new InvalidOperationException("Categoría no encontrada.");

        if (category.IsActive)
        {
            var hasActiveProducts = await _productRepository.AnyAsync(
                [p => p.CategoryId == id && p.IsActive]);

            if (hasActiveProducts)
                throw new InvalidOperationException("No se puede dar de baja la categoría porque tiene productos activos asociados.");
        }

        category.IsActive = !category.IsActive;
        await _categoryRepository.EditAsync(category);
        return category.IsActive;
    }

}
