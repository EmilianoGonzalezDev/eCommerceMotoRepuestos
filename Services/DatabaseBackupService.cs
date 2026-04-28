using Microsoft.Data.Sqlite;

namespace eCommerceMotoRepuestos.Services;

public class DatabaseBackupService(SqliteDatabasePathProvider databasePathProvider)
{
    private const string BackupDirectoryName = "Backups";
    private const string BackupFileName = "motoRepuestos_backup.db";

    public string GetBackupFilePath()
    {
        var backupDirectory = Path.Combine(databasePathProvider.BaseDirectory, BackupDirectoryName);
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

        await using var connection = new SqliteConnection(databasePathProvider.ConnectionString);
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

        await using var destinationConnection = new SqliteConnection(databasePathProvider.ConnectionString);
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

    private string GetDatabaseFilePath()
    {
        return databasePathProvider.DatabaseFilePath;
    }
}
