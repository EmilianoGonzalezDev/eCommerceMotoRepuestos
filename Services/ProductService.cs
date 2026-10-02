using eCommerceMotoRepuestos.Entities;
using eCommerceMotoRepuestos.Models;
using eCommerceMotoRepuestos.Repositories;
using eCommerceMotoRepuestos.Utilities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace eCommerceMotoRepuestos.Services;

public class ProductService(
    GenericRepository<Category> _categoryRepository,
    GenericRepository<Product> _productRepository,
    IWebHostEnvironment _webHostEnvironment
    )
{
    public async Task<PagedResult<ProductViewModel>> GetAdminPagedAsync(
        int page,
        int pageSize,
        ProductSortBy sortBy,
        SortDirection sortDir,
        string search,
        bool lowStockOnly,
        int lowStockThreshold)
    {
        var query = _productRepository.Query();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{EscapeLikePattern(search)}%";
            query = query.Where(x => EF.Functions.Like(x.Name, pattern, LikeEscapeChar));
        }

        if (lowStockOnly)
        {
            query = query.Where(x => x.Stock <= lowStockThreshold);
        }

        var isDesc = sortDir == SortDirection.Desc;
        // SQLite cannot ORDER BY decimal columns, so Price is sorted as REAL.
        query = sortBy switch
        {
            ProductSortBy.Category => isDesc
                ? query.OrderByDescending(x => x.Category!.Name).ThenBy(x => x.Name)
                : query.OrderBy(x => x.Category!.Name).ThenBy(x => x.Name),
            ProductSortBy.Price => isDesc
                ? query.OrderByDescending(x => (double)x.Price).ThenBy(x => x.Name)
                : query.OrderBy(x => (double)x.Price).ThenBy(x => x.Name),
            ProductSortBy.Stock => isDesc
                ? query.OrderByDescending(x => x.Stock).ThenBy(x => x.Name)
                : query.OrderBy(x => x.Stock).ThenBy(x => x.Name),
            _ => isDesc
                ? query.OrderByDescending(x => x.Name)
                : query.OrderBy(x => x.Name)
        };

        var projected = query.Select(product => new ProductViewModel
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
        });

        return await PagedResult<ProductViewModel>.CreateAsync(projected, page, pageSize);
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

    public async Task<Dictionary<int, ProductViewModel>> GetByIdsAsync(IEnumerable<int> ids)
    {
        var idList = ids.Distinct().ToList();
        if (idList.Count == 0)
        {
            return new Dictionary<int, ProductViewModel>();
        }

        var products = await _productRepository.GetAllProjectedAsync(product => new ProductViewModel
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
        },
        conditions: [p => idList.Contains(p.ProductId)]);

        return products.ToDictionary(product => product.ProductId);
    }

    public async Task PopulateCategoriesAsync(ProductViewModel productVM)
    {
        var selectedCategoryId = productVM.Category.CategoryId;
        var categories = await _categoryRepository.GetAllProjectedAsync(
            c => new { c.CategoryId, c.Name },
            conditions: [c => c.IsActive || c.CategoryId == selectedCategoryId]);
        productVM.Categories = categories.Select(category => new SelectListItem
        {
            Value = category.CategoryId.ToString(),
            Text = category.Name,
        }).OrderBy(s => s.Text).ToList();
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
            ImageName = viewModel.ImageName,
            SearchText = SearchTextNormalizer.ForProduct(viewModel.Name, viewModel.Description)
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
        product.SearchText = SearchTextNormalizer.ForProduct(viewModel.Name, viewModel.Description);

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

    public async Task<PagedResult<ProductViewModel>> GetCatalogPagedAsync(int page, int pageSize, int categoryId = 0, string search = "")
    {
        var query = BuildCatalogQuery(categoryId, search)
            .OrderByDescending(item => item.ProductId)
            .Select(item => new ProductViewModel
            {
                ProductId = item.ProductId,
                IsActive = item.IsActive,
                Name = item.Name,
                Description = item.Description,
                Price = item.Price,
                Stock = item.Stock,
                ImageName = item.ImageName,
            });

        return await PagedResult<ProductViewModel>.CreateAsync(query, page, pageSize);
    }

    /// <summary>
    /// Returns the best <paramref name="limit"/> catalog matches for the search box, ranked by
    /// exact match, prefix match, then shorter names. Only id, name and price are read.
    /// </summary>
    public async Task<List<ProductViewModel>> GetSearchSuggestionsAsync(string search, int limit)
    {
        var searchValue = (search ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(searchValue)) return [];

        var matches = await BuildCatalogQuery(0, searchValue)
            .Select(x => new { x.ProductId, x.Name, x.Price })
            .ToListAsync();

        return matches
            .OrderByDescending(p => p.Name.Equals(searchValue, StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(p => p.Name.StartsWith(searchValue, StringComparison.OrdinalIgnoreCase))
            .ThenBy(p => p.Name.Length)
            .ThenBy(p => p.Name)
            .Take(limit)
            .Select(p => new ProductViewModel
            {
                ProductId = p.ProductId,
                Name = p.Name,
                Price = p.Price
            })
            .ToList();
    }

    private IQueryable<Product> BuildCatalogQuery(int categoryId, string? search)
    {
        var query = _productRepository.Query()
            .Where(x => x.Stock > 0 && x.IsActive && x.Category != null && x.Category.IsActive);

        if (categoryId != 0) query = query.Where(x => x.CategoryId == categoryId);

        foreach (var term in SearchTextNormalizer.SplitTerms(search))
        {
            query = query.Where(x => x.SearchText.Contains(term));
        }

        return query;
    }

    private const string LikeEscapeChar = "\\";

    private static string EscapeLikePattern(string value)
    {
        return value.Trim()
            .Replace(LikeEscapeChar, LikeEscapeChar + LikeEscapeChar)
            .Replace("%", LikeEscapeChar + "%")
            .Replace("_", LikeEscapeChar + "_");
    }
}
