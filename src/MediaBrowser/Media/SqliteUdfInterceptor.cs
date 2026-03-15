namespace MediaBrowser.Media;

public class SqliteUdfInterceptor : DbConnectionInterceptor
{
    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData) => RegisterUdfs((SqliteConnection)connection);

    public override Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken ct = default)
    {
        RegisterUdfs((SqliteConnection)connection);
        return Task.CompletedTask;
    }

    public static void RegisterUdfs(SqliteConnection connection)
    {
        connection.CreateFunction("seeded_random", (long seed, Guid id) =>
        {
            var rng = new Random((int)(seed ^ id.GetHashCode()));
            return rng.NextDouble();
        });
    }
}