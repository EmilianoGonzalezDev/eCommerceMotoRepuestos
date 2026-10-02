using Microsoft.EntityFrameworkCore;

namespace eCommerceMotoRepuestos.Models;

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages => PageSize == 0 ? 0 : (int)System.Math.Ceiling((double)TotalItems / PageSize);
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;

    public static PagedResult<T> Create(IEnumerable<T> source, int pageNumber, int pageSize)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;

        var totalItems = source.Count();
        var totalPages = pageSize == 0 ? 0 : (int)System.Math.Ceiling((double)totalItems / pageSize);
        if (totalPages > 0 && pageNumber > totalPages) pageNumber = totalPages;
        var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return new PagedResult<T>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    /// <summary>
    /// Pages a database query: runs a COUNT and then fetches only the requested page.
    /// The query must already be ordered.
    /// </summary>
    public static async Task<PagedResult<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;

        var totalItems = await source.CountAsync();
        var totalPages = (int)System.Math.Ceiling((double)totalItems / pageSize);
        if (totalPages > 0 && pageNumber > totalPages) pageNumber = totalPages;
        var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<T>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }
}
