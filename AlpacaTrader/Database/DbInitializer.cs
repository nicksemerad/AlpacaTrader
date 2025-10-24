using Npgsql;

namespace Database;

/// <summary>
///   Handles database schema initialization.
/// </summary>
public class DbInitializer
{
    private readonly TradingDbConnection _tradingDbConnection;

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

        const string createTableSql = """

                                                  CREATE TABLE IF NOT EXISTS bars (
                                                      id SERIAL PRIMARY KEY,
                                                      symbol VARCHAR(10) NOT NULL,
                                                      timestamp TIMESTAMP NOT NULL,
                                                      open DECIMAL(18, 6) NOT NULL,
                                                      high DECIMAL(18, 6) NOT NULL,
                                                      low DECIMAL(18, 6) NOT NULL,
                                                      close DECIMAL(18, 6) NOT NULL,
                                                      volume BIGINT NOT NULL,
                                                      trade_count INTEGER NOT NULL,
                                                      vwap DECIMAL(18, 6) NOT NULL,
                                                      UNIQUE(symbol, timestamp)
                                                  );
                                                  
                                                  CREATE INDEX IF NOT EXISTS idx_bars_symbol_timestamp 
                                                  ON bars(symbol, timestamp);
                                      """;

        await using var cmd = new NpgsqlCommand(createTableSql, connection);
        await cmd.ExecuteNonQueryAsync();

        Console.WriteLine("Database tables initialized successfully.");
    }
}