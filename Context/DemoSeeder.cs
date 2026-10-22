using eCommerceMotoRepuestos.Services;

namespace eCommerceMotoRepuestos.Context;

public static class DemoSeeder
{
    private const string SeedDirectoryName = "SeedData";
    private const string SeedDatabaseFileName = "seed.db";
    private const string ImagesDirectoryName = "images";

    /// <summary>
    /// Copies the versioned demo database when no database exists yet (e.g. first start on Azure),
    /// and restores any missing demo product images into wwwroot/images.
    /// Must run before migrations so they are applied on top of the seed.
    /// </summary>
    public static void EnsureSeeded(SqliteDatabasePathProvider databasePathProvider, IWebHostEnvironment environment)
    {
        var seedDirectory = Path.Combine(databasePathProvider.BaseDirectory, SeedDirectoryName);

        var databaseFilePath = databasePathProvider.DatabaseFilePath;
        Directory.CreateDirectory(Path.GetDirectoryName(databaseFilePath)!);

        var seedDatabasePath = Path.Combine(seedDirectory, SeedDatabaseFileName);
        if (!File.Exists(databaseFilePath) && File.Exists(seedDatabasePath))
        {
            File.Copy(seedDatabasePath, databaseFilePath);
        }

        var seedImagesDirectory = Path.Combine(seedDirectory, ImagesDirectoryName);
        if (!Directory.Exists(seedImagesDirectory)) return;

        var imagesDirectory = Path.Combine(environment.WebRootPath, ImagesDirectoryName);
        Directory.CreateDirectory(imagesDirectory);

        foreach (var seedImagePath in Directory.EnumerateFiles(seedImagesDirectory))
        {
            var imagePath = Path.Combine(imagesDirectory, Path.GetFileName(seedImagePath));
            if (!File.Exists(imagePath))
            {
                File.Copy(seedImagePath, imagePath);
            }
        }
    }
}
