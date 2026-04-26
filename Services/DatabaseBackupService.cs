using Microsoft.Data.Sqlite;

namespace eCommerceMotoRepuestos.Services;

public class DatabaseBackupService(IConfiguration configuration, IWebHostEnvironment environment)
{
    private const string ConnectionStringName = "SqlString";
    private const string BackupDirectoryName = "Backups";
    private const string BackupFileName = "motoRepuestos_backup.db";

    public string GetBackupFilePath()
    {
        var backupDirectory = Path.Combine(environment.ContentRootPath, BackupDirectoryName);
        return Path.Combine(backupDirectory, BackupFileName);
    }

    public async Task<string> CreateBackupAsync(CancellationToken cancellationToken = default)
    {
        var backupFilePath = GetBackupFilePath();
        var databaseFilePath = GetDatabaseFilePath();

        Directory.CreateDirectory(Path.GetDirectoryName(backupFilePath)!);
        if (!File.Exists(databaseFilePath))
        {
            throw new FileNotFoundException("No se encontro el archivo de base de datos SQLite.", databaseFilePath);
        }

        if (File.Exists(backupFilePath))
        {
            File.Delete(backupFilePath);
        }

        SqliteConnection.ClearAllPools();

        await using var connection = new SqliteConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);

        var backupConnectionString = new SqliteConnectionStringBuilder
        {
            DataSource = backupFilePath
        }.ToString();

        await using var backupConnection = new SqliteConnection(backupConnectionString);
        await backupConnection.OpenAsync(cancellationToken);

        connection.BackupDatabase(backupConnection);
        return backupFilePath;
    }

    public async Task RestoreBackupAsync(CancellationToken cancellationToken = default)
    {
        var backupFilePath = GetBackupFilePath();
        if (!File.Exists(backupFilePath))
        {
            throw new FileNotFoundException("No se encontro un backup para restaurar.", backupFilePath);
        }

        var databaseFilePath = GetDatabaseFilePath();
        Directory.CreateDirectory(Path.GetDirectoryName(databaseFilePath)!);

        SqliteConnection.ClearAllPools();

        var backupConnectionString = new SqliteConnectionStringBuilder
        {
            DataSource = backupFilePath
        }.ToString();

        await using var connection = new SqliteConnection(backupConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var destinationConnection = new SqliteConnection(GetConnectionString());
        await destinationConnection.OpenAsync(cancellationToken);

        connection.BackupDatabase(destinationConnection);
        SqliteConnection.ClearAllPools();
    }

    public DateTime? GetBackupLastWriteUtc()
    {
        var backupFilePath = GetBackupFilePath();
        if (!File.Exists(backupFilePath))
        {
            return null;
        }

        return File.GetLastWriteTimeUtc(backupFilePath);
    }

    private string GetConnectionString()
    {
        return configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException($"No se encontro la cadena de conexion '{ConnectionStringName}'.");
    }

    private string GetDatabaseFilePath()
    {
        var builder = new SqliteConnectionStringBuilder(GetConnectionString());
        if (string.IsNullOrWhiteSpace(builder.DataSource))
        {
            throw new InvalidOperationException("La cadena de conexion de SQLite no contiene Data Source.");
        }

        return Path.IsPathRooted(builder.DataSource)
            ? builder.DataSource
            : Path.GetFullPath(Path.Combine(environment.ContentRootPath, builder.DataSource));
    }
}
