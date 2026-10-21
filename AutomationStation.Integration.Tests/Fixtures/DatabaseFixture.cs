using Dapper;

using Respawn;

using Testcontainers.MsSql;

using Microsoft.Data.SqlClient;

using AutomationStation.Infrastructure.Database;

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

        private Respawner _respawner = null!;

        /// <summary>
        /// Method <c>InitializeAsync</c> runs before the tests to create the database and implements the tables through Dapper
        /// </summary>
        /// 
        public async ValueTask InitializeAsync()
        {

            try
            {
                await _sqlServer.StartAsync();

                await using var connection = new SqlConnection(_sqlServer.GetConnectionString());
                await connection.OpenAsync();

                await connection.ExecuteAsync(
                    "CREATE DATABASE AutomationStationTestDb;");

                var connectionString = new SqlConnectionStringBuilder(
                    _sqlServer.GetConnectionString())
                {
                    InitialCatalog = "AutomationStationTestDb"
                };

                ConnectionFactory = new DbConnectionFactory(connectionString.ConnectionString);

                await using var databaseConnection = ConnectionFactory.CreateConnection();

                await databaseConnection.OpenAsync();

                var schemaPath = Path.Combine(AppContext.BaseDirectory, "Database", "V1__create_tables.sql");

                var schemaSql = await File.ReadAllTextAsync(schemaPath);

                await databaseConnection.ExecuteAsync(schemaSql);

                _respawner = await Respawner.CreateAsync(
                    databaseConnection,
                    new RespawnerOptions
                    {
                        DbAdapter = DbAdapter.SqlServer,
                        SchemasToInclude = ["dbo"]
                    });
            }
            catch
            {
                await _sqlServer.DisposeAsync();
                throw;
            }
        }

        public async Task ResetAsync()
        {
            await using var connection = ConnectionFactory.CreateConnection();
            await connection.OpenAsync();

            await _respawner.ResetAsync(connection);
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
