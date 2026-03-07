using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using Npgsql;
using Shouldly;

namespace MediaBrowser;

public static partial class TestDatabaseCleaner
{
    public async static Task CleanDatabase(this MediaBrowserWebApplicationFactory factory, CancellationToken cancellationToken = default)
    {
        // We need to read the configuration settings for the connection strings
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.Properties.Add(Installer.CliArgsKey, Array.Empty<string>());
        configurationBuilder.Properties.Add(Installer.TestConfigsKey, factory.ConfigurationFiles.ToArray());
        Installer.ConfigureSettings(configurationBuilder);

        var dbConfig = new DbConfig(configurationBuilder.Build());
        var connectionString = factory.DbType switch
        {
            DbType.MySql => dbConfig.MySqlConnectionString,
            DbType.Postgres => dbConfig.PostgresConnectionString,
            DbType.SqlServer => dbConfig.SqlServerConnectionString,
            _ => dbConfig.SqliteConnectionString
        };
        connectionString.ShouldNotBeNullOrEmpty("Missing the connection string for this test.");

        // get the connection string without the database name
        var dbConnectionString = DatabaseRegex().Replace(connectionString, ";").TrimStart(';');
        // get the database name from the connection string so we can delete it then recreate it
        var dbName = DatabaseRegex().Match(connectionString).Groups["value"].Value;
        if (factory.DbType != DbType.Sqlite)
        {
            dbName.ShouldNotBeNullOrEmpty("The database name is missing from the connection string.");
        }

        switch (factory.DbType)
        {
            case DbType.MySql:
                await CleanMySql(dbConnectionString, dbName, cancellationToken);
                break;
            case DbType.Postgres:
                await CleanPostgres(dbConnectionString, dbName, cancellationToken);
                break;
            case DbType.Sqlite:
                await CleanSqlite(connectionString, cancellationToken);
                break;
            case DbType.SqlServer:
                await CleanSqlServer(dbConnectionString, dbName, cancellationToken);
                break;
        }
    }

    async static private Task CleanMySql(string connectionString, string dbName, CancellationToken cancellationToken = default)
    {
        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using (var command = connection.CreateCommand())
        {
            command.CommandText = $"DROP DATABASE IF EXISTS {dbName};";
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await using (var command = connection.CreateCommand())
        {
            command.CommandText = $"CREATE DATABASE {dbName}; ";
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    async static private Task CleanPostgres(string connectionString, string dbName, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using (var command = connection.CreateCommand())
        {
            command.CommandText = $"DROP DATABASE {dbName};";
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await using (var command = connection.CreateCommand())
        {
            command.CommandText = $"CREATE DATABASE {dbName}; ";
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    async static private Task CleanSqlite(string connectionString, CancellationToken cancellationToken = default)
    {
        // NOTE: For SQLite, we can't drop the database since it's just a file,
        // so instead we need to drop all tables in the database to clean it.
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var tables = new List<string>();
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';";
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                tables.Add(reader.GetString(0));
            }
        }

        foreach (var table in tables)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = $"DROP TABLE IF EXISTS \"{table}\"";
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    async static private Task CleanSqlServer(string connectionString, string dbName, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using (var command = connection.CreateCommand())
        {
            command.CommandText = $"DROP DATABASE IF EXISTS {dbName};";
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await using (var command = connection.CreateCommand())
        {
            command.CommandText = $"CREATE DATABASE {dbName}; ";
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    /// <summary>
    /// This pattern captures the database name from the connection string.
    /// </summary>
    [GeneratedRegex("(^|;|)Database=(?<value>[^;]+)(;|$)", RegexOptions.IgnoreCase, "en-US")]
    static private partial Regex DatabaseRegex();
}
