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
            Name = item.Name
        }
        ).ToList();

        return categoriesVieModel;
    }

    public async Task AddAsync(CategoryViewModel viewModel)
    {
        var entity = new Category
        {
            Name = viewModel.Name
        };

        await _categoryRepository.AddAsync(entity);
    }

    public async Task<CategoryViewModel?> GetEditViewModelAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);

        if (category == null) return null;

        var categoryViewModel = new CategoryViewModel
        {
            Name = category.Name,
            CategoryId = category.CategoryId
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
        var entity = new Category
        {
            CategoryId = viewModel.CategoryId,
            Name = viewModel.Name
        };

        await _categoryRepository.EditAsync(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        var productsInCategory = await _productRepository.GetAllAsync(
            conditions: [p => p.CategoryId == id]);

        if (productsInCategory.Any())
            throw new InvalidOperationException("No se puede eliminar la categoría porque tiene productos asociados.");

        await _categoryRepository.DeleteAsync(category);
    }

}
