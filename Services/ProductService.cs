using eCommerceMotoRepuestos.Entities;
using eCommerceMotoRepuestos.Models;
using eCommerceMotoRepuestos.Repositories;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;
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
                //includes: new Expression<Func<Product, object>>[] { x => x.Category! } //versión del video
                includes: [x => x.Category!] //versión resumida
            );


        var productsVM = products.Select(product =>
            new ProductViewModel
            {
                ProductId = product.ProductId,
                Category = new CategoryViewModel
                {
                    CategoryId = product.Category!.CategoryId,
                    Name = product.Category.Name,
                },
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                ImageName = product.ImageName,
            }).ToList();

        return productsVM;
    }

    public async Task<ProductViewModel> GetByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        var categories = await _categoryRepository.GetAllAsync();
        var productVM = new ProductViewModel();

        if (product != null)
        {
            productVM = new ProductViewModel
            {
                ProductId = product.ProductId,
                Category = new CategoryViewModel
                {
                    CategoryId = product.Category!.CategoryId,
                    Name = product.Category.Name,
                },
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                ImageName = product.ImageName
            };

        }

        productVM.Categories = categories.Select(category => new SelectListItem
        {
            Value = category.CategoryId.ToString(),
            Text = category.Name,

        }).ToList();

        return productVM;
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
            Name = viewModel.Name,
            Description = viewModel.Description,
            Price = viewModel.Price,
            Stock = viewModel.Stock,
            ImageName = viewModel.ImageName
        };

        await _productRepository.AddAsync(entity);
    }

    public async Task EditAsync(ProductViewModel viewModel)
    {

        var product = await _productRepository.GetByIdAsync(viewModel.ProductId);

        if (viewModel.ImageFile != null)
        {

            string uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(viewModel.ImageFile.FileName);
            string filePath = Path.Combine(uploadFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
                await viewModel.ImageFile.CopyToAsync(fileStream);

            if (!product.ImageName.IsNullOrEmpty())
            {
                var previousImage = product.ImageName;
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
    }
    public async Task DeleteAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        await _productRepository.DeleteAsync(product!);
    }

    public async Task<IEnumerable<ProductViewModel>> GetCatalogAsync(int categoryId = 0, string search = "")
    {

        var conditions = new List<Expression<Func<Product, bool>>>
            {
                x => x.Stock > 0
            };

        if (categoryId != 0) conditions.Add(x => x.CategoryId == categoryId);
        if (!string.IsNullOrEmpty(search)) conditions.Add(x => x.Name.Contains(search));


        var products = await _productRepository.GetAllAsync(conditions: conditions.ToArray());

        var productsVM = products.Select(item =>
            new ProductViewModel
            {
                ProductId = item.ProductId,
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                Stock = item.Stock,
                ImageName = item.ImageName,
            }).ToList();

        return productsVM;
    }
}
