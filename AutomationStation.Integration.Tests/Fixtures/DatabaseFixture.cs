using AutomationStation.Infrastructure.Database;
using Dapper;
using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;

namespace AutomationStation.Integration.Tests.Fixtures
{
    /// <summary>
    /// Class <c>DatabaseFixture</c> does the setup and cleanup of the testdatabase
    /// </summary>
    public sealed class DatabaseFixture : IAsyncLifetime
    {
        private readonly MsSqlContainer _sqlServer = new MsSqlBuilder(
            "mcr.microsoft.com/mssql/server:2022-latest")
            .Build();
        public DbConnectionFactory ConnectionFactory { get; private set; } = null!;

        /// <summary>
        /// Method <c>InitializeAsync</c> runs before the tests to create the database and implements the tables through Dapper
        /// </summary>
        /// 
        public async ValueTask InitializeAsync()
        {

            try
            {
                await _sqlServer.StartAsync();

                using var connection = new SqlConnection(_sqlServer.GetConnectionString());
                await connection.OpenAsync();

                await connection.ExecuteAsync(
                    "CREATE DATABASE AutomationStationTestDb;");

                var connectionString = new SqlConnectionStringBuilder(
                    _sqlServer.GetConnectionString())
                {
                    InitialCatalog = "AutomationStationTestDb"
                };

                ConnectionFactory = new DbConnectionFactory(connectionString.ConnectionString);

                using var databaseConnection = ConnectionFactory.CreateConnection();

                await databaseConnection.OpenAsync();

                var schemaPath = Path.Combine(AppContext.BaseDirectory, "Database", "V1__create_tables.sql");

                var schemaSql = await File.ReadAllTextAsync(schemaPath);

                await databaseConnection.ExecuteAsync(schemaSql);
            }
            catch
            {
                await _sqlServer.DisposeAsync();
                throw;
            }
        }

        /// <summary>
        /// Method <c>DisposeAsync</c> cleans up the database automatically when the tests are done
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            await _sqlServer.DisposeAsync();
        }
    }
}
