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
            var cancellationToken = TestContext.Current.CancellationToken;

            var automationId = Guid.NewGuid();
            var actionId = Guid.NewGuid();
            var missingExecutorId = Guid.NewGuid();

            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            await connection.ExecuteAsync(
                new CommandDefinition(
                    """
                    INSERT INTO Automations (Id, Name)
                    VALUES (@Id, @Name);
                    """,
                    new { Id = automationId, Name = "Test automation" },
                    transaction: transaction,
                    cancellationToken: cancellationToken));

            var exception = await Assert.ThrowsAsync<SqlException>(() =>
                connection.ExecuteAsync(
                    new CommandDefinition(
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
                        transaction: transaction,
                        cancellationToken: cancellationToken)));

            Assert.Equal(547, exception.Number);
        }

        [Fact]
        public async Task InsertAction_WithValidExecutorId_IsInsertedSuccessfully()
        {
            var cancellationToken = TestContext.Current.CancellationToken;

            var automationId = Guid.NewGuid();
            var actionId = Guid.NewGuid();
            var executorId = Guid.NewGuid();

            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            await connection.ExecuteAsync(
                new CommandDefinition(
                    """
                    INSERT INTO Automations (Id, Name)
                    VALUES (@Id, @Name);
                    """,
                    new { Id = automationId, Name = "Test automation" },
                    transaction: transaction,
                    cancellationToken: cancellationToken));

            await connection.ExecuteAsync(
                new CommandDefinition(
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
                    transaction: transaction,
                    cancellationToken: cancellationToken));

            var storedExecutorId = await connection.QuerySingleAsync<Guid>(
                new CommandDefinition(
                    """
                    SELECT AutomationExecutorId
                    FROM AutomationActions
                    WHERE Id = @Id;
                    """,
                    new { Id = actionId },
                    transaction: transaction,
                    cancellationToken: cancellationToken));

            Assert.Equal(executorId, storedExecutorId);
        }
    }
}
