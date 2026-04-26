using eCommerceMotoRepuestos.Entities;
using eCommerceMotoRepuestos.Models;
using eCommerceMotoRepuestos.Repositories;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq.Expressions;

namespace eCommerceMotoRepuestos.Services;

public class ProductService(
    GenericRepository<Category> _categoryRepository,
    GenericRepository<Product> _productRepository,
    IWebHostEnvironment _webHostEnvironment
    )
{
    public async Task<IEnumerable<ProductViewModel>> GetAllAsync()
    {
        var products = await _productRepository.GetAllAsync(
                includes: [x => x.Category!]
            );


        var productsVM = products.Select(product =>
            new ProductViewModel
            {
                ProductId = product.ProductId,
                IsActive = product.IsActive,
                Category = new CategoryViewModel
                {
                    CategoryId = product.Category!.CategoryId,
                    Name = product.Category.Name,
                    IsActive = product.Category.IsActive
                },
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                ImageName = product.ImageName,
            }).ToList();

        return productsVM;
    }

    public async Task<ProductViewModel> GetAddViewModelAsync()
    {
        var productVM = new ProductViewModel
        {
            Category = new CategoryViewModel()
        };

        await PopulateCategoriesAsync(productVM);
        return productVM;
    }

    public async Task<ProductViewModel?> GetEditViewModelAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null) return null;

        var productVM = new ProductViewModel
        {
            ProductId = product.ProductId,
            IsActive = product.IsActive,
            Category = new CategoryViewModel
            {
                CategoryId = product.CategoryId
            },
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            ImageName = product.ImageName
        };

        await PopulateCategoriesAsync(productVM);
        return productVM;
    }

    public async Task<ProductViewModel> GetByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            return new ProductViewModel
            {
                Category = new CategoryViewModel()
            };
        }

        return new ProductViewModel
        {
            ProductId = product.ProductId,
            IsActive = product.IsActive,
            Category = new CategoryViewModel
            {
                CategoryId = product.CategoryId
            },
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            ImageName = product.ImageName
        };
    }

    public async Task PopulateCategoriesAsync(ProductViewModel productVM)
    {
        var categories = await _categoryRepository.GetAllAsync(
            conditions: [c => c.IsActive || c.CategoryId == productVM.Category.CategoryId]);
        productVM.Categories = categories.Select(category => new SelectListItem
        {
            Value = category.CategoryId.ToString(),
            Text = category.Name,
        }).ToList();
    }

    public async Task AddAsync(ProductViewModel viewModel)
    {

        if (viewModel.ImageFile != null)
        {
            string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(viewModel.ImageFile.FileName);
            string filePath = Path.Combine(uploadFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
                await viewModel.ImageFile.CopyToAsync(fileStream);

            viewModel.ImageName = uniqueFileName;
        }

        var entity = new Product
        {
            CategoryId = viewModel.Category.CategoryId,
            IsActive = true,
            Name = viewModel.Name,
            Description = viewModel.Description,
            Price = viewModel.Price,
            Stock = viewModel.Stock,
            ImageName = viewModel.ImageName
        };

        await _productRepository.AddAsync(entity);
    }

    public async Task<bool> EditAsync(ProductViewModel viewModel)
    {

        var product = await _productRepository.GetByIdAsync(viewModel.ProductId);

        if (product == null)
        { 
            return false;
        }
        
        if (viewModel.ImageFile != null)
        {

            string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(viewModel.ImageFile.FileName);
            string filePath = Path.Combine(uploadFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
                await viewModel.ImageFile.CopyToAsync(fileStream);

            if (product.ImageName is string previousImage)
            {
                string deleteFilePath = Path.Combine(uploadFolder, previousImage);

                if (File.Exists(deleteFilePath)) File.Delete(deleteFilePath);
            }

            viewModel.ImageName = uniqueFileName;
        }
        else
        {
            viewModel.ImageName = product.ImageName;
        }

        product.CategoryId = viewModel.Category.CategoryId;
        product.Name = viewModel.Name;
        product.Description = viewModel.Description;
        product.Price = viewModel.Price;
        product.Stock = viewModel.Stock;
        product.ImageName = viewModel.ImageName;

        await _productRepository.EditAsync(product);

        return true;
    }
    public async Task<bool> ToggleActiveAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product is null) throw new InvalidOperationException("Producto no encontrado.");

        product.IsActive = !product.IsActive;
        await _productRepository.EditAsync(product);
        return product.IsActive;
    }

    public async Task<IEnumerable<ProductViewModel>> GetCatalogAsync(int categoryId = 0, string search = "")
    {

        var conditions = new List<Expression<Func<Product, bool>>>
            {
                x => x.Stock > 0,
                x => x.IsActive,
                x => x.Category != null && x.Category.IsActive
            };

        if (categoryId != 0) conditions.Add(x => x.CategoryId == categoryId);
        if (!string.IsNullOrEmpty(search)) conditions.Add(x => x.Name.Contains(search));


        var products = await _productRepository.GetAllAsync(conditions: conditions.ToArray());

        var productsVM = products.Select(item =>
            new ProductViewModel
            {
                ProductId = item.ProductId,
                IsActive = item.IsActive,
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                Stock = item.Stock,
                ImageName = item.ImageName,
            }).ToList();

        return productsVM;
    }
}
