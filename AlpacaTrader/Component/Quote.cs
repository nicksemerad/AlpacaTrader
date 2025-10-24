using Common;

namespace Component;

public class Quote
{
  public DateTime Timestamp { get; set; }
  
  public QuoteSide Side { get; set; } // ask or bid
  
  public string Exchange { get; set; }
  // Q = NASDAQ
  // N = NYSE
  // P = NYSE arca
  // Z = BATS
  // " " = No asks available
  
  public decimal Price { get; set; }
  
  public int Size { get; set; }
  
  public string Tape { get; set; } // exchange that the security trades on
  // A = NYSE securities
  // B = NYSE arca and regional exchanges
  // C = NASDAQ

  public Quote(DateTime timestamp, QuoteSide side, string exchange, decimal price, int size, string tape)
  {
    Timestamp = timestamp;
    Side = side;
    Exchange = exchange;
    Price = price;
    Size = size;
    Tape = tape;
  }
}