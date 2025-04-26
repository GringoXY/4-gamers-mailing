using Npgsql;

namespace Infrastructure.Options;

/// <summary>
/// Represents PostgreSQL section in appsettings.*.json
/// </summary>
public class PostgreSQLOptions
{
    /// <summary>
    /// Section name of settings related to PostgreSQL
    /// </summary>
    public static readonly string SectionName = "PostgreSQL";

    /// <summary>
    /// Database name
    /// </summary>
    public string Database { get; set; } = string.Empty;

    /// <summary>
    /// Where DB is located
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// Port where DB is located
    /// </summary>
    public int Port { get; set; } = 5432;

    /// <summary>
    /// Login to DB
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Password to DB
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Connection string to DB
    /// </summary>
    public string ConnectionString => _connectionString.ToString();

    private NpgsqlConnectionStringBuilder _connectionString => new()
    {
        Database = Database,
        Host = Host,
        Port = Port,
        Username = Username,
        Password = Password,
    };
}
