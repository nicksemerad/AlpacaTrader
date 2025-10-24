using System.ComponentModel;
using System.Reflection;

namespace Common;

// Equity trading: market, limit, stop, stop_limit, trailing_stop.
// Options trading: market, limit.
// Multileg Options trading: market, limit.
// Crypto trading: market, limit, stop_limit.
public enum OrderType
{
    [Description("market")] Market,
    [Description("limit")] Limit,
    [Description("stop")] Stop,
    [Description("stop_limit")] StopLimit,
    [Description("trailing_stop")] TrailingStop
}

/*
 * day: rest of the day, or queued to start next day
 * gtc: good until cancelled
 * opg: only valid during open, cancelled after
 * cls: only valid during close
 * ioc: immediately or cancel
 * foc: fill or kill, either totally filled or cancelled
 */
public enum TimeInForce
{
    [Description("day")] Day,
    [Description("gtc")] Gtc,
    [Description("opg")] Opg,
    [Description("cls")] Cls,
    [Description("ioc")] Ioc,
    [Description("foc")] Fok
}

// if the order is sell or buy
public enum OrderSide
{
    [Description("buy")] Buy,
    [Description("sell")] Sell
}

public enum QuoteSide
{
    [Description("ask")] Ask,
    [Description("bid")] Bid
}

public static class EnumExtensions
{
    public static string ToDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field?.GetCustomAttribute<DescriptionAttribute>();
        return attribute?.Description ?? value.ToString();
    }
}