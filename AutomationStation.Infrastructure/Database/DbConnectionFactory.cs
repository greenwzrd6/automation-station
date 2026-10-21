using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

using System.Data.Common;

namespace AutomationStation.Infrastructure.Database
{
    /// <summary>
    /// Creates unopened SQL Server connections for the AutomationStation infrastructure layer.
    /// </summary>
    /// <remarks>Connections are created from a single connection string and must be opened by the caller, either
    /// explicitly or through an async Dapper command that opens them on demand. The factory itself is stateless and
    /// safe to share; each call to <see cref="CreateConnection"/> returns a new <see cref="SqlConnection"/>.</remarks>
    /// <param name="connectionString">The SQL Server connection string used for every created connection.</param>
    public class DbConnectionFactory(string connectionString)
    {
        private readonly string _connectionString = connectionString;

        /// <summary>
        /// Initializes a factory from application configuration, reading the "DefaultConnection" connection string.
        /// </summary>
        /// <param name="configuration">Configuration that supplies the connection string.</param>
        /// <exception cref="InvalidOperationException">The "DefaultConnection" connection string is missing.</exception>
        public DbConnectionFactory(IConfiguration configuration)
            : this(configuration.GetConnectionString("DefaultConnection")
                  ?? throw new InvalidOperationException("Connection string not found."))
        {
        }

        /// <summary>
        /// Creates a new, unopened SQL Server connection using the configured connection string.
        /// </summary>
        /// <returns>A <see cref="SqlConnection"/> that the caller owns and must dispose.</returns>
        public DbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
