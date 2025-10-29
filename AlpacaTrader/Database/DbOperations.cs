using Common;
using Component;
using Npgsql;

namespace Database;

/// <summary>
///   This class holds database operations for saving and retrieving rows from the different tables in trading_db.
/// </summary>
public static class DbOperations
{
    /// <summary>
    ///   Inserts a bar into the database.
    /// </summary>
    public static async Task InsertBarAsync(Bar bar)
    {
        var tradingDbConnection = new TradingDbConnection();
        // get a connection and make a new command using the InsertBar sql command
        await using var connection = await tradingDbConnection.GetConnectionAsync();
        await using var cmd = new NpgsqlCommand(SqlCommands.InsertBar, connection);

        // add the InsertBar parameters to the command, which takes in all the bar properties
        cmd.Parameters.AddWithValue("symbol", bar.Symbol);
        cmd.Parameters.AddWithValue("timeframe", bar.Timeframe);
        cmd.Parameters.AddWithValue("date", bar.Date);
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
    ///   Inserts all the bars in the parameter list into the database.
    /// </summary>
    public static async Task InsertBarsAsync(List<Bar> bars)
    {
        foreach (var bar in bars)
            await InsertBarAsync(bar);
    }

    /// <summary>
    ///   Gets the bars for a symbol within a time range from the database.
    /// </summary>
    public static async Task<List<Bar>> GetBarsBySymbolTimeframeAsync(string symbol, string timeframe,
        DateTime startTime, DateTime endTime)
    {
        var tradingDbConnection = new TradingDbConnection();
        // get a connection and make a new command with the GetBarsBySymbolTimeframeDate sql query
        await using var connection = await tradingDbConnection.GetConnectionAsync();
        await using var cmd = new NpgsqlCommand(SqlCommands.GetBarsBySymbolTimeframeDate, connection);

        // add the GetBarsBySymbolTimeframeDate parameters to the command
        cmd.Parameters.AddWithValue("symbol", symbol);
        cmd.Parameters.AddWithValue("timeframe", timeframe);
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
                Timeframe = reader.GetString(1),
                Date = reader.GetDateTime(2),
                Open = reader.GetDecimal(3),
                High = reader.GetDecimal(4),
                Low = reader.GetDecimal(5),
                Close = reader.GetDecimal(6),
                Volume = reader.GetDecimal(7),
                TradeCount = reader.GetDecimal(8),
                VolumeWeightedAverage = reader.GetDecimal(9)
            });
        }

        return bars;
    }

    /// <summary>
    ///   Inserts a calendar day into the database.
    /// </summary>
    public static async Task InsertCalendarDayAsync(CalendarDay day)
    {
        var tradingDbConnection = new TradingDbConnection();
        // get a connection and make a new command using the InsertCalendarDay sql command
        await using var connection = await tradingDbConnection.GetConnectionAsync();
        await using var cmd = new NpgsqlCommand(SqlCommands.InsertCalendarDay, connection);

        // add the InsertCalendarDay parameters to the command, which takes in all the CalendarDay properties
        cmd.Parameters.AddWithValue("date", day.Date.ToDateTime(TimeOnly.MinValue));
        cmd.Parameters.AddWithValue("openTime", day.OpenTime.ToTimeSpan());
        cmd.Parameters.AddWithValue("closeTime", day.CloseTime.ToTimeSpan());
        cmd.Parameters.AddWithValue("sessionOpenTime", day.SessionOpenTime.ToTimeSpan());
        cmd.Parameters.AddWithValue("sessionCloseTime", day.SessionCloseTime.ToTimeSpan());

        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    ///   Inserts all the calendar days in the parameter list into the database.
    /// </summary>
    public static async Task InsertCalendarDaysAsync(List<CalendarDay> days)
    {
        foreach (var day in days)
            await InsertCalendarDayAsync(day);
    }

    /// <summary>
    ///   Gets a calendar day with the date parameter from the database, or null if it isn't found.
    /// </summary>
    public static async Task<CalendarDay?> GetCalendarDayAsync(DateOnly date)
    {
        var tradingDbConnection = new TradingDbConnection();
        // get a connection and make a new command with the GetCalendarDayByDate sql query
        await using var connection = await tradingDbConnection.GetConnectionAsync();
        await using var cmd = new NpgsqlCommand(SqlCommands.GetCalendarDayByDate, connection);

        // add the GetCalendarDayByDate parameter to the command
        cmd.Parameters.AddWithValue("date", date.ToDateTime(TimeOnly.MinValue));

        await using var reader = await cmd.ExecuteReaderAsync();

        // if the reader found the CalendarDay row, use the values to make and return a new CalendarDay
        if (await reader.ReadAsync())
        {
            return new CalendarDay
            {
                Date = DateOnly.FromDateTime(reader.GetDateTime(0)),
                OpenTime = TimeOnly.FromTimeSpan(reader.GetTimeSpan(1)),
                CloseTime = TimeOnly.FromTimeSpan(reader.GetTimeSpan(2)),
                SessionOpenTime = TimeOnly.FromTimeSpan(reader.GetTimeSpan(3)),
                SessionCloseTime = TimeOnly.FromTimeSpan(reader.GetTimeSpan(4))
            };
        }

        // the reader had nothing to read, so the CalendarDay wasn't found. return null
        return null;
    }

    /// <summary>
    ///   Gets all calendar days within a date range from the database.
    /// </summary>
    public static async Task<List<CalendarDay>> GetCalendarDaysAsync(DateOnly startDate, DateOnly endDate)
    {
        var tradingDbConnection = new TradingDbConnection();
        // get a connection and make a new command with the GetCalendarDaysByDateRange sql query
        await using var connection = await tradingDbConnection.GetConnectionAsync();
        await using var cmd = new NpgsqlCommand(SqlCommands.GetCalendarDaysByDateRange, connection);

        // add the GetCalendarDaysByDateRange parameters to the command
        cmd.Parameters.AddWithValue("startDate", startDate.ToDateTime(TimeOnly.MinValue));
        cmd.Parameters.AddWithValue("endDate", endDate.ToDateTime(TimeOnly.MinValue));

        // make a list to hold the CalendarDays and get a reader for the data from executing the command
        var days = new List<CalendarDay>();
        await using var reader = await cmd.ExecuteReaderAsync();

        // for each item in the reader make a new CalendarDay with the data and add it to days
        while (await reader.ReadAsync())
        {
            days.Add(new CalendarDay
            {
                Date = DateOnly.FromDateTime(reader.GetDateTime(0)),
                OpenTime = TimeOnly.FromTimeSpan(reader.GetTimeSpan(1)),
                CloseTime = TimeOnly.FromTimeSpan(reader.GetTimeSpan(2)),
                SessionOpenTime = TimeOnly.FromTimeSpan(reader.GetTimeSpan(3)),
                SessionCloseTime = TimeOnly.FromTimeSpan(reader.GetTimeSpan(4))
            });
        }

        return days;
    }
    
    /// <summary>
    ///   Gets the total number of bars in the bars database table as a decimal. If the sql command returns null, -1m
    ///   is returned instead.
    /// </summary>
    /// <returns>The number of bars in the bars table or -1 if the sql command fails</returns>
    public static async Task<decimal> GetBarsCountAsync()
    {
        var tradingDbConnection = new TradingDbConnection();
        // get a connection and make a new command with the GetBarsCount sql query
        await using var connection = await tradingDbConnection.GetConnectionAsync();
        await using var cmd = new NpgsqlCommand(SqlCommands.GetBarsCount, connection);
        
        // execute the command and return the first and only row, if the row is null return -1
        var result = await cmd.ExecuteScalarAsync();
        return result != null ? Convert.ToDecimal(result) : -1m;
    }
    
    /// <summary>
    ///   Gets the total number of CalendarDays in the trading_calendar database table as a decimal. If the sql command
    ///   returns null, -1m is returned instead.
    /// </summary>
    /// <returns>The number of CalendarDays in the trading_calendar table or -1 if the sql command fails</returns>
    public static async Task<decimal> GetCalendarDaysCountAsync()
    {
        var tradingDbConnection = new TradingDbConnection();
        // get a connection and make a new command with the GetCalendarDaysCount sql query
        await using var connection = await tradingDbConnection.GetConnectionAsync();
        await using var cmd = new NpgsqlCommand(SqlCommands.GetCalendarDaysCount, connection);
        
        // execute the command and return the first and only row, if the row is null return -1
        var result = await cmd.ExecuteScalarAsync();
        return result != null ? Convert.ToDecimal(result) : -1m;
    }
}