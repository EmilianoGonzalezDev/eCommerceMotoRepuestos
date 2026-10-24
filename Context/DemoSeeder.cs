using eCommerceMotoRepuestos.Services;

namespace eCommerceMotoRepuestos.Context;

public static class DemoSeeder
{
    private const string SeedDirectoryName = "SeedData";
    private const string SeedDatabaseFileName = "seed.db";
    private const string ImagesDirectoryName = "images";
    private const string DefaultImageFileName = "default.png";

    /// <summary>
    /// Copies the versioned demo database when no database exists yet (e.g. first start on Azure),
    /// and restores any missing demo product images into wwwroot/images.
    /// Must run before migrations so they are applied on top of the seed.
    /// </summary>
    public static void EnsureSeeded(SqliteDatabasePathProvider databasePathProvider, IWebHostEnvironment environment)
    {
        var databaseFilePath = databasePathProvider.DatabaseFilePath;
        Directory.CreateDirectory(Path.GetDirectoryName(databaseFilePath)!);

        var seedDatabasePath = GetSeedDatabasePath(databasePathProvider);
        if (!File.Exists(databaseFilePath) && File.Exists(seedDatabasePath))
        {
            File.Copy(seedDatabasePath, databaseFilePath);
        }

        SyncImages(databasePathProvider, environment, removeUnknown: false);
    }

    public static string GetSeedDatabasePath(SqliteDatabasePathProvider databasePathProvider)
    {
        return Path.Combine(GetSeedDirectory(databasePathProvider), SeedDatabaseFileName);
    }

    /// <summary>
    /// Restores missing demo images into wwwroot/images. With <paramref name="removeUnknown"/>,
    /// also deletes every image that is not part of the seed (uploads made during the demo).
    /// </summary>
    public static void SyncImages(SqliteDatabasePathProvider databasePathProvider, IWebHostEnvironment environment, bool removeUnknown)
    {
        var seedImagesDirectory = Path.Combine(GetSeedDirectory(databasePathProvider), ImagesDirectoryName);
        if (!Directory.Exists(seedImagesDirectory)) return;

        var imagesDirectory = Path.Combine(environment.WebRootPath, ImagesDirectoryName);
        Directory.CreateDirectory(imagesDirectory);

        var seedImageNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { DefaultImageFileName };

        foreach (var seedImagePath in Directory.EnumerateFiles(seedImagesDirectory))
        {
            var imageName = Path.GetFileName(seedImagePath);
            seedImageNames.Add(imageName);

            var imagePath = Path.Combine(imagesDirectory, imageName);
            if (!File.Exists(imagePath))
            {
                File.Copy(seedImagePath, imagePath);
            }
        }

        if (!removeUnknown) return;

        foreach (var imagePath in Directory.EnumerateFiles(imagesDirectory))
        {
            if (!seedImageNames.Contains(Path.GetFileName(imagePath)))
            {
                File.Delete(imagePath);
            }
        }
    }

    private static string GetSeedDirectory(SqliteDatabasePathProvider databasePathProvider)
    {
        return Path.Combine(databasePathProvider.BaseDirectory, SeedDirectoryName);
    }
}
