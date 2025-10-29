namespace Common;

public class SqlCommands
{
    public const string InsertBar = """
        INSERT INTO bars (symbol, timeframe, date, open, high, low, close, volume, trade_count, vwap)
        VALUES (@symbol, @timeframe, @date, @open, @high, @low, @close, @volume, @tradeCount, @vwap)
        ON CONFLICT (symbol, timeframe, date) DO NOTHING
    """;

    public const string GetBarsBySymbolTimeframeDate = """
        SELECT symbol, timeframe, date, open, high, low, close, volume, trade_count, vwap
        FROM bars
        WHERE symbol = @symbol AND timeframe = @timeframe AND date BETWEEN @startTime AND @endTime
        ORDER BY date
    """;

    public const string CreateBarsTable = """
        CREATE TABLE IF NOT EXISTS bars (
            id SERIAL PRIMARY KEY,
            symbol VARCHAR(10) NOT NULL,
            timeframe VARCHAR(10) NOT NULL,
            date TIMESTAMP NOT NULL,
            open DECIMAL(18, 6) NOT NULL,
            high DECIMAL(18, 6) NOT NULL,
            low DECIMAL(18, 6) NOT NULL,
            close DECIMAL(18, 6) NOT NULL,
            volume DECIMAL(18, 6) NOT NULL,
            trade_count DECIMAL(18, 6) NOT NULL,
            vwap DECIMAL(18, 6) NOT NULL,
            UNIQUE(symbol, timeframe, date)
        );
       CREATE INDEX IF NOT EXISTS idx_bars_symbol_timeframe_date
       ON bars(symbol, timeframe, date);
    """;
    
    public const string InsertCalendarDay = """
        INSERT INTO trading_calendar (date, open_time, close_time, session_open_time, session_close_time)
        VALUES (@date, @open_time, @close_time, @session_open_time,  @session_close_time)
        ON CONFLICT (date) DO NOTHING
    """;
    
    public const string GetCalendarDayByDate = """
        SELECT date, open_time, close_time, session_open_time, session_close_time
        FROM trading_calendar
        WHERE date = @date
    """;

    public const string GetCalendarDaysByDateRange = """
        SELECT date, open_time, close_time, session_open_time, session_close_time
        FROM trading_calendar
        WHERE date BETWEEN @startDate AND @endDate
        ORDER BY date
    """;

    public const string CreateCalendarTable = """
        CREATE TABLE IF NOT EXISTS trading_calendar (
            date DATE PRIMARY KEY,
            open_time TIME NOT NULL,
            close_time TIME NOT NULL,
            session_open_time TIME NOT NULL,
            session_close_time TIME NOT NULL
        );
        CREATE INDEX IF NOT EXISTS idx_calendar_date 
        ON trading_calendar(date);
    """;
}