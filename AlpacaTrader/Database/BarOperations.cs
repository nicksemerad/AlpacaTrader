using Common;
using Component;
using Npgsql;

namespace Database;

/// <summary>
///   This class holds database operations for Bars i.e. saving and retrieving
/// </summary>
public class BarOperations
{
    private readonly TradingDbConnection _tradingDbConnection;

    /// <summary>
    ///   Makes a new BarOperations class to handle database operations with Bars
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
        // get a connection and make a new command using the InsertBar query with parameter values
        await using var connection = await _tradingDbConnection.GetConnectionAsync();
        await using var cmd = new NpgsqlCommand(SqlQueries.InsertBar, connection);
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
        // get a connection and make a new command using the GetBarsBySymbol query with parameter values
        await using var connection = await _tradingDbConnection.GetConnectionAsync();
        await using var cmd = new NpgsqlCommand(SqlQueries.GetBarsBySymbol, connection);
        cmd.Parameters.AddWithValue("symbol", symbol);
        cmd.Parameters.AddWithValue("startTime", startTime);
        cmd.Parameters.AddWithValue("endTime", endTime);

        // make a list to hold the bars and get a reader for the data from executing the command
        var bars = new List<Bar>();
        await using var reader = await cmd.ExecuteReaderAsync();

        // for each item in the reader make a new bar with the data and add it to bars
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
                Volume = reader.GetInt32(6),
                TradeCount = reader.GetInt32(7),
                VolumeWeightedAverage = reader.GetDecimal(8)
            });
        }

        return bars;
    }
}