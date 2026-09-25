using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data.Common;

namespace AutomationStation.Infrastructure.Database;

public class DbConnectionFactory(string connectionString)
{
    private readonly string _connectionString = connectionString;

    public DbConnectionFactory(IConfiguration configuration)
        : this(configuration.GetConnectionString("DefaultConnection")
              ?? throw new InvalidOperationException("Connection string not found."))
    {
    }

    public DbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}