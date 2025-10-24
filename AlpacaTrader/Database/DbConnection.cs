using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Database;

/// <summary>
///   This class handles connecting to the PostgreSQL database.
/// </summary>
public class DbConnection
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
    public DbConnection()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddUserSecrets<DbConnection>()
            .Build();

        _connectionString = configuration.GetConnectionString("TradingDb")
                            ?? throw new InvalidOperationException();
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
}