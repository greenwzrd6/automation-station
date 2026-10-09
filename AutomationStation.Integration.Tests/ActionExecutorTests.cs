using AutomationStation.Integration.Tests.Fixtures;
using Dapper;
using Microsoft.Data.SqlClient;

namespace AutomationStation.Integration.Tests
{
    [Collection(DatabaseCollection.Name)]
    public class ActionExecutorTests(DatabaseFixture database)
    {
        private readonly DatabaseFixture _database = database;

        [Fact]
        public async Task InsertAction_WithMissingExecutorId_IsRejectedByForeignKey()
        {

            var automationId = Guid.NewGuid();
            var actionId = Guid.NewGuid();
            var missingExecutorId = Guid.NewGuid();

            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var transaction = connection.BeginTransaction();

            await connection.ExecuteAsync(
                """
                INSERT INTO Automations (Id, Name)
                VALUES (@Id, @Name);
                """,
                new { Id = automationId, Name = "Test automation" },
                transaction);

            var exception = await Assert.ThrowsAsync<SqlException>(() =>
                connection.ExecuteAsync(
                    """
                    INSERT INTO AutomationActions
                        (Id, AutomationId, AutomationExecutorId, ActionType)
                    VALUES
                        (@Id, @AutomationId, @ExecutorId, @ActionType);
                    """,
                    new
                    {
                        Id = actionId,
                        AutomationId = automationId,
                        ExecutorId = missingExecutorId,
                        ActionType = "CreateTask"
                    },
                    transaction));

            Assert.Equal(547, exception.Number);
        }

        [Fact]
        public async Task InsertAction_WithValidExecutorId_IsInsertedSuccessfully()
        {
            var automationId = Guid.NewGuid();
            var actionId = Guid.NewGuid();
            var executorId = Guid.NewGuid();

            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            await connection.ExecuteAsync(
                """
                INSERT INTO Automations (Id, Name)
                VALUES (@Id, @Name);
                """,
                new { Id = automationId, Name = "Test automation" },
                transaction);

            await connection.ExecuteAsync(
                """
                INSERT INTO AutomationExecutors (Id, Name)
                VALUES (@Id, @Name);
                """,
                new { Id = executorId, Name = "Test executor" },
                transaction);

            await connection.ExecuteAsync(
                """
                INSERT INTO AutomationActions
                    (Id, AutomationId, AutomationExecutorId, ActionType)
                VALUES
                    (@Id, @AutomationId, @ExecutorId, @ActionType);
                """,
                new
                {
                    Id = actionId,
                    AutomationId = automationId,
                    ExecutorId = executorId,
                    ActionType = "CreateTask"
                },
                transaction);

            transaction.Commit();
        }
    }
}
