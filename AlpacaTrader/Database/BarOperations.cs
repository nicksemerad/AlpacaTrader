using Component;
using Npgsql;

namespace Database;

public class BarOperations
{
    private readonly TradingDbConnection _tradingDbConnection;

    /// <summary>
    ///   Makes a new BarOperations class to 
    /// </summary>
    public BarOperations()
    {
        _tradingDbConnection = new TradingDbConnection();
    }

    /// <summary>
    ///   Inserts a bar into the database.
    /// </summary>
    public async Task InsertBarAsync(Bar bar)
    {
        await using var connection = await _tradingDbConnection.GetConnectionAsync();

        const string sql = @"
            INSERT INTO bars (symbol, timestamp, open, high, low, close, volume, trade_count, vwap)
            VALUES (@symbol, @timestamp, @open, @high, @low, @close, @volume, @tradeCount, @vwap)
            ON CONFLICT (symbol, timestamp) DO NOTHING";

        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("symbol", bar.Symbol);
        cmd.Parameters.AddWithValue("timestamp", bar.Timestamp);
        cmd.Parameters.AddWithValue("open", bar.Open);
        cmd.Parameters.AddWithValue("high", bar.High);
        cmd.Parameters.AddWithValue("low", bar.Low);
        cmd.Parameters.AddWithValue("close", bar.Close);
        cmd.Parameters.AddWithValue("volume", bar.Volume);
        cmd.Parameters.AddWithValue("tradeCount", bar.TradeCount);
        cmd.Parameters.AddWithValue("vwap", bar.VolumeWeightedAverage);

        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    ///   Inserts all bars in a list into the database.
    /// </summary>
    public async Task InsertBarsAsync(List<Bar> bars)
    {
        foreach (var bar in bars)
            await InsertBarAsync(bar);
    }

    /// <summary>
    ///   Retrieves bars for a symbol within a time range from the database.
    /// </summary>
    public async Task<List<Bar>> GetBarsBySymbolAsync(string symbol, DateTime startTime, DateTime endTime)
    {
        await using var connection = await _tradingDbConnection.GetConnectionAsync();

        const string sql = """

                                       SELECT symbol, timestamp, open, high, low, close, volume, trade_count, vwap
                                       FROM bars
                                       WHERE symbol = @symbol AND timestamp BETWEEN @startTime AND @endTime
                                       ORDER BY timestamp
                           """;

        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("symbol", symbol);
        cmd.Parameters.AddWithValue("startTime", startTime);
        cmd.Parameters.AddWithValue("endTime", endTime);

        var bars = new List<Bar>();
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            bars.Add(new Bar
            {
                Symbol = reader.GetString(0),
                Timestamp = reader.GetDateTime(1),
                Open = reader.GetDecimal(2),
                High = reader.GetDecimal(3),
                Low = reader.GetDecimal(4),
                Close = reader.GetDecimal(5),
                Volume = reader.GetInt64(6),
                TradeCount = reader.GetInt32(7),
                VolumeWeightedAverage = reader.GetDecimal(8)
            });
        }

        return bars;
    }
}