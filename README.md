# AlpacaTrader

## About
Welcome to my AlpacaTrader Repo! This is a C# pet project, and is a work-in-progress. The end
goal is for this solution to be an algo-trader that is fully hands-off, making trades in
various financial markets without any intervention. Additionally it will contain a 
backtesting engine that will test how strategies would have performed if they were running 
in the past. 

This is obviously a daunting task and I am not planning on finishing it any time soon, 
but working towards the ultimate goal has been a great way to learn. As time goes on it will
most likely expand to use more APIs besides Alpaca, trade more than stocks, etc. 


## Solution Projects

### Api
Handles requesting endpoints, parsing JSON responses, and storing the parsed Alpaca API data 
in the database.

### Database
Handles storing and retrieving Alpaca API data using a Postgres SQL database.

### Backtest
Tests and evaluates strategy's performance by simulating live trading using historical data.
Currently unimplemented. 

### Strategy
Contains the different trading strategies that dictate when and how to buy, hold, or sell 
assets, based on price data from the Alpaca API.

### Indicators
Calculates technical indicators with Alpaca API data using the Skender.Stock.Indicators 
package. Only has SMA, EMA, MACD, RSI, and Bollinger Bands currently. More indicators will
be added when needed by new strategies.

### Component
Contains custom C# objects which represent components necessary for trading assets. The
properties of each object correspond with data from the Alpaca API, and some are used when
deserializing JSON responses. For example, the most used component is the Bar. A Bar
represents a bar/ candle/ quote or whatever you want to call it for:

1. a symbol
2. a timeframe (how long each bar represents i.e. 1-min, 1-day, etc.)
3. a single date and time

That means each bar has the prices at open, high, close, and low, for the parameters, as
well as the volume of shares traded, number of trades, and the volume weighted average price.

### Common
Functionality used by all projects. Includes the enums, logger factory, API endpoint builders,
and utils for working with DateTime objects. The utils are used to convert between time zones,
format dates into strings that work with API requests, etc.

## Current Test Projects

### ApiTests
Only test project right now as Api has been my main focus. More to come.

