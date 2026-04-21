using Microsoft.Data.SqlClient;

namespace eCommerceMotoRepuestos.Services;

public class DatabaseBackupService(IConfiguration configuration, IWebHostEnvironment environment)
{
    private const string ConnectionStringName = "SqlString";
    private const string BackupDirectoryName = "Backups";
    private const string BackupFileName = "motoRepuestos_backup.bak";

    public string GetBackupFilePath()
    {
        var backupDirectory = Path.Combine(environment.ContentRootPath, BackupDirectoryName);
        return Path.Combine(backupDirectory, BackupFileName);
    }

    public async Task<string> CreateBackupAsync(CancellationToken cancellationToken = default)
    {
        var connectionString = GetConnectionString();
        var databaseName = GetDatabaseName(connectionString);
        var backupFilePath = GetBackupFilePath();

        Directory.CreateDirectory(Path.GetDirectoryName(backupFilePath)!);

        var escapedDatabaseName = EscapeSqlIdentifier(databaseName);
        var escapedBackupPath = EscapeSqlLiteral(backupFilePath);

        var sql = $"""
            BACKUP DATABASE [{escapedDatabaseName}]
            TO DISK = N'{escapedBackupPath}'
            WITH INIT, FORMAT, NAME = N'{escapedDatabaseName}-Full Backup', STATS = 10;
            """;

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection)
        {
            CommandTimeout = 0
        };

        await command.ExecuteNonQueryAsync(cancellationToken);
        return backupFilePath;
    }

    public async Task RestoreBackupAsync(CancellationToken cancellationToken = default)
    {
        var backupFilePath = GetBackupFilePath();
        if (!File.Exists(backupFilePath))
        {
            throw new FileNotFoundException("No se encontro un backup para restaurar.", backupFilePath);
        }

        var connectionString = GetConnectionString();
        var databaseName = GetDatabaseName(connectionString);
        var escapedDatabaseName = EscapeSqlIdentifier(databaseName);
        var escapedBackupPath = EscapeSqlLiteral(backupFilePath);

        var masterConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = "master"
        };

        SqlConnection.ClearAllPools();

        await using var connection = new SqlConnection(masterConnectionStringBuilder.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        var restoreSql = $"""
            ALTER DATABASE [{escapedDatabaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
            RESTORE DATABASE [{escapedDatabaseName}] FROM DISK = N'{escapedBackupPath}' WITH REPLACE, RECOVERY;
            ALTER DATABASE [{escapedDatabaseName}] SET MULTI_USER;
            """;

        try
        {
            await ExecuteNonQueryAsync(connection, restoreSql, cancellationToken);
        }
        catch
        {
            var setMultiUserSql = $"ALTER DATABASE [{escapedDatabaseName}] SET MULTI_USER;";
            await ExecuteNonQueryAsync(connection, setMultiUserSql, cancellationToken);
            throw;
        }
        finally
        {
            SqlConnection.ClearAllPools();
        }
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

    private static string GetDatabaseName(string connectionString)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        if (string.IsNullOrWhiteSpace(builder.InitialCatalog))
        {
            throw new InvalidOperationException("La cadena de conexion no tiene nombre de base de datos (Initial Catalog).");
        }

        return builder.InitialCatalog;
    }

    private static string EscapeSqlLiteral(string value) => value.Replace("'", "''");
    private static string EscapeSqlIdentifier(string value) => value.Replace("]", "]]");

    private static async Task ExecuteNonQueryAsync(SqlConnection connection, string sql, CancellationToken cancellationToken)
    {
        await using var command = new SqlCommand(sql, connection)
        {
            CommandTimeout = 0
        };

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
