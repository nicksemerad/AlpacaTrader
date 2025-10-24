using Common;
using Npgsql;

namespace Database;

/// <summary>
///   Handles database schema initialization.
/// </summary>
public class DbInitializer
{
    private readonly TradingDbConnection _tradingDbConnection;

    /// <summary>
    ///   Creates a new Database initializer and gets a connection.
    /// </summary>
    public DbInitializer()
    {
        _tradingDbConnection = new TradingDbConnection();
    }

    /// <summary>
    ///   Creates the bars table if it doesn't exist.
    /// </summary>
    public async Task InitializeDatabaseAsync()
    {
        await using var connection = await _tradingDbConnection.GetConnectionAsync();

        await using var cmd = new NpgsqlCommand(SqlQueries.CreateBarsTable, connection);
        await cmd.ExecuteNonQueryAsync();

        Console.WriteLine("Database tables initialized successfully.");
    }
}