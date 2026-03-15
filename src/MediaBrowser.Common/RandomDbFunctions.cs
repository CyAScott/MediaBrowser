namespace MediaBrowser;

/// <summary>
/// EF Core database function mappings for seeded random ordering.
/// Functions are provider-specific — only call functions matching your configured provider.
/// </summary>
public static class RandomDbFunctions
{
    /// <summary>
    /// MySQL: Returns a seeded random double. Behaviour in ORDER BY varies by version.
    /// </summary>
    /// <example>RAND(@seed)</example>
    [DbFunction("RAND", IsNullable = false)]
    public static double MySqlRand(int seed) =>
        throw new NotSupportedException("EF Core translation only.");

    /// <summary>
    /// PostgreSQL: Returns a random double in [0.0, 1.0).
    /// Call PgSetSeed() in a prior ExecuteSqlRawAsync to make this deterministic.
    /// </summary>
    /// <example>SELECT random()</example>
    [DbFunction("random", IsNullable = false)]
    public static double PgRandom() =>
        throw new NotSupportedException("EF Core translation only.");

    /// <summary>
    /// SQLite: Seeded random order using a registered UDF.
    /// Registered via SqliteUdfInterceptor — not a native SQLite function.
    /// </summary>
    /// <example>seeded_random(@seed, id)</example>
    [DbFunction("seeded_random", IsNullable = false)]
    public static double SqliteSeededRandom(long seed, Guid rowId) =>
        throw new NotSupportedException("EF Core translation only.");

    /// <summary>
    /// SQL Server: Returns an integer checksum of the provided values.
    /// </summary>
    /// <example>CHECKSUM(@seed, [id])</example>
    [DbFunction("CHECKSUM", IsNullable = false)]
    public static int SqlChecksum(int seed, Guid value) =>
        throw new NotSupportedException("EF Core translation only.");
}