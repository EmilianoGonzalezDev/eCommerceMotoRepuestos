using eCommerceMotoRepuestos.Utilities;
using Microsoft.EntityFrameworkCore;

namespace eCommerceMotoRepuestos.Context;

public static class DatabaseInitializer
{
    /// <summary>
    /// Applies pending migrations and fills derived columns that cannot be computed in SQL.
    /// Runs at startup and after a backup restore (an older backup may predate recent migrations).
    /// </summary>
    public static async Task InitializeAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        await db.Database.MigrateAsync(cancellationToken);
        await BackfillProductSearchTextAsync(db, cancellationToken);
    }

    private static async Task BackfillProductSearchTextAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        var products = await db.Product
            .Where(x => x.SearchText == string.Empty)
            .ToListAsync(cancellationToken);

        if (products.Count == 0) return;

        foreach (var product in products)
        {
            product.SearchText = SearchTextNormalizer.ForProduct(product.Name, product.Description);
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
