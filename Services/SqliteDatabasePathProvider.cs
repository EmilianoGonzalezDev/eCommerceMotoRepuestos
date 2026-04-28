using Microsoft.Data.Sqlite;

namespace eCommerceMotoRepuestos.Services;

public class SqliteDatabasePathProvider(IConfiguration configuration)
{
    private const string ConnectionStringName = "SqlString";

    public string BaseDirectory => AppContext.BaseDirectory;

    public string ConnectionString { get; } = BuildConnectionString(configuration);

    public string DatabaseFilePath { get; } = ResolveDatabaseFilePath(configuration);

    private static string BuildConnectionString(IConfiguration configuration)
    {
        var builder = new SqliteConnectionStringBuilder(GetConfiguredConnectionString(configuration));
        if (string.IsNullOrWhiteSpace(builder.DataSource))
        {
            throw new InvalidOperationException("La cadena de conexion de SQLite no contiene Data Source.");
        }

        builder.DataSource = Path.IsPathRooted(builder.DataSource)
            ? builder.DataSource
            : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, builder.DataSource));

        return builder.ToString();
    }

    private static string ResolveDatabaseFilePath(IConfiguration configuration)
    {
        var builder = new SqliteConnectionStringBuilder(BuildConnectionString(configuration));
        return builder.DataSource;
    }

    private static string GetConfiguredConnectionString(IConfiguration configuration)
    {
        return configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException($"No se encontro la cadena de conexion '{ConnectionStringName}'.");
    }
}
