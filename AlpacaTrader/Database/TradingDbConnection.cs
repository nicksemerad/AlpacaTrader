using Common;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Database;

/// <summary>
///   This class handles connecting to the PostgreSQL database.
/// </summary>
public class TradingDbConnection
{
    /// <summary>
    ///   Connection string for TradingDb.
    /// </summary>
    private readonly string _connectionString;

    /// <summary>
    ///   Makes the connection string using the user secrets i.e. Host, Port, Database, Username, Password.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///   If something goes wrong when getting the connection string
    /// </exception>
    public TradingDbConnection()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddUserSecrets<TradingDbConnection>()
            .Build();

        _connectionString = configuration.GetConnectionString("TradingDb")
                            ?? throw new InvalidOperationException("Failed to get connection string.");
    }

    /// <summary>
    ///   Uses the connection string to connect to the database and return a connection object.
    /// </summary>
    /// <returns>The NpgsqlConnection to the database</returns>
    public async Task<NpgsqlConnection> GetConnectionAsync()
    {
        NpgsqlConnection connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }

    /// <summary>
    ///   Returns a boolean determining if the database connected successfully.
    /// </summary>
    /// <returns>True if the database connection succeeded, else false</returns>
    public async Task<bool> IsDbConnectedAsync()
    {
        try
        {
            await using NpgsqlConnection connection = await GetConnectionAsync();
            Console.WriteLine("Database connection succeeded");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database connection failed: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    ///   Initializes the database tables, creates them if they don't already exist. Should be called once when the
    ///   TradingDbConnection is first connected.
    /// </summary>
    public async Task InitializeDatabaseAsync()
    {
        await using var connection = await GetConnectionAsync();
        
        await using var cmd = new NpgsqlCommand(SqlQueries.CreateBarsTable, connection);
        await cmd.ExecuteNonQueryAsync();
        
        Console.WriteLine("Database tables initialized successfully.");
    }
}