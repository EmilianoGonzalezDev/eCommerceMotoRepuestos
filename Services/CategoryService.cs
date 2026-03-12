using eCommerceMotoRepuestos.Entities;
using eCommerceMotoRepuestos.Models;
using eCommerceMotoRepuestos.Repositories;
using Microsoft.EntityFrameworkCore;

namespace eCommerceMotoRepuestos.Services;

public class CategoryService(
    GenericRepository<Category> _categoryRepository,
    GenericRepository<Product> _productRepository)
{
    public async Task<IEnumerable<CategoryViewModel>> GetAllAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();

        var categoriesVieModel = categories.Select(item =>
        new CategoryViewModel
        {
            CategoryId = item.CategoryId,
            Name = item.Name,
            IsActive = item.IsActive
        }
        ).ToList();

        return categoriesVieModel;
    }

    public async Task<IEnumerable<CategoryViewModel>> GetAllActiveAsync()
    {
        var categories = await _categoryRepository.GetAllAsync(
            conditions: [c => c.IsActive]);

        var categoriesVieModel = categories.Select(item =>
        new CategoryViewModel
        {
            CategoryId = item.CategoryId,
            Name = item.Name,
            IsActive = item.IsActive
        }
        ).ToList();

        return categoriesVieModel;
    }

    public async Task AddAsync(CategoryViewModel viewModel)
    {
        var entity = new Category
        {
            Name = viewModel.Name,
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
        var category = await _categoryRepository.GetByIdAsync(id);
        return category != null;
    }

    public async Task EditAsync(CategoryViewModel viewModel)
    {
        var category = await _categoryRepository.GetByIdAsync(viewModel.CategoryId);
        if (category is null) return;

        category.Name = viewModel.Name;
        await _categoryRepository.EditAsync(category);
    }

    public async Task<bool> ToggleActiveAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category is null) throw new InvalidOperationException("Categoría no encontrada.");

        if (category.IsActive)
        {
            var productsInCategory = await _productRepository.GetAllAsync(
                conditions: [p => p.CategoryId == id && p.IsActive]);

            if (productsInCategory.Any())
                throw new InvalidOperationException("No se puede dar de baja la categoría porque tiene productos activos asociados.");
        }

        category.IsActive = !category.IsActive;
        await _categoryRepository.EditAsync(category);
        return category.IsActive;
    }

}
