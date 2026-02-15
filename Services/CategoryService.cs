using eCommerceMotoRepuestos.Entities;
using eCommerceMotoRepuestos.Models;
using eCommerceMotoRepuestos.Repositories;
using Microsoft.EntityFrameworkCore;

namespace eCommerceMotoRepuestos.Services;

public class CategoryService(GenericRepository<Category> _categoryRepository)
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

    public async Task<CategoryViewModel?> GetByIdAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        var categoryViewModel = new CategoryViewModel();

        if (category != null)
        {
            categoryViewModel.Name = category.Name;
            categoryViewModel.CategoryId = category.CategoryId;
        }

        return categoryViewModel;
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
        var category = await _categoryRepository.GetByIdAsync(id) ?? throw new Exception("Categoría no encontrada");
        await _categoryRepository.DeleteAsync(category);
    }

}
