namespace Common;

public class SqlQueries
{
    public const string InsertBar = """
        INSERT INTO bars (symbol, timestamp, open, high, low, close, volume, trade_count, vwap)
        VALUES (@symbol, @timestamp, @open, @high, @low, @close, @volume, @tradeCount, @vwap)
        ON CONFLICT (symbol, timestamp) DO NOTHING
        """;

    public const string GetBarsBySymbol = """
        SELECT symbol, timestamp, open, high, low, close, volume, trade_count, vwap
        FROM bars
        WHERE symbol = @symbol AND timestamp BETWEEN @startTime AND @endTime
        ORDER BY timestamp
        """;

    public const string CreateBarsTable = """
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
    
}