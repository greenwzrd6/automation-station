using Dapper;

using System.Data.Common;

using AutomationStation.Infrastructure.Persistence;
using AutomationStation.Integration.Tests.Fixtures;

namespace AutomationStation.Integration.Tests.Persistence
{
    [Collection(DatabaseCollection.Name)]
    public sealed class AutomationExecutionRepositoryTests(DatabaseFixture database) : IAsyncLifetime
    {
        private readonly DatabaseFixture _database = database;
        private readonly AutomationExecutionRepository _repository = new(
            database.ConnectionFactory);

        public async ValueTask InitializeAsync()
        {
            await _database.ResetAsync();
        }

        public ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }

        [Fact]
        public async Task GetOrCreateAsync_WhenNoExecutionExists_ShouldCreateNewExecution()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;

            var automationId = Guid.NewGuid();
            var eventId = Guid.NewGuid();

            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await InsertAutomationAsync(connection, automationId, cancellationToken);

            var beforeInsert = await TimestampAsync(connection, cancellationToken);

            // Act
            var executionId = await _repository.GetOrCreateAsync(automationId, eventId, cancellationToken);

            var afterInsert = await TimestampAsync(connection, cancellationToken);

            // Assert
            var executionRow = await GetAutomationExecutionRowAsync(connection, automationId, eventId, cancellationToken);

            Assert.Equal(executionId, executionRow.Id);

            Assert.Equal(automationId, executionRow.AutomationId);

            Assert.Equal(eventId, executionRow.EventId);

            Assert.Null(executionRow.AutomationExecutorId);

            Assert.InRange(executionRow.StartedAt, beforeInsert.AddSeconds(-1), afterInsert.AddSeconds(1));
        }

        [Fact]
        public async Task GetOrCreateAsync_WhenExecutionAlreadyExists_ShouldReturnSameId()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;

            var automationId = Guid.NewGuid();
            var eventId = Guid.NewGuid();

            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await InsertAutomationAsync(connection, automationId, cancellationToken);

            // Act
            var executionId1 = await _repository.GetOrCreateAsync(automationId, eventId, cancellationToken);
            var executionId2 = await _repository.GetOrCreateAsync(automationId, eventId, cancellationToken);

            // Assert
            Assert.Equal(executionId1, executionId2);

            var row = await GetAutomationExecutionRowAsync(connection, automationId, eventId, cancellationToken);

            Assert.Equal(executionId1, row.Id);
        }

        [Fact]
        public async Task GetOrCreateAsync_WhenEventIdIsDifferent_ShouldCreateNewExecution()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;

            var automationId = Guid.NewGuid();
            var eventId1 = Guid.NewGuid();
            var eventId2 = Guid.NewGuid();

            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await InsertAutomationAsync(connection, automationId, cancellationToken);

            // Act
            var executionId1 = await _repository.GetOrCreateAsync(automationId, eventId1, cancellationToken);
            var executionId2 = await _repository.GetOrCreateAsync(automationId, eventId2, cancellationToken);

            // Assert
            Assert.NotEqual(executionId1, executionId2);
        }

        [Fact]
        public async Task GetOrCreateAsync_WhenAutomationIdIsDifferent_ShouldCreateNewExecution()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;

            var automationId1 = Guid.NewGuid();
            var automationId2 = Guid.NewGuid();
            var eventId = Guid.NewGuid();

            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await InsertAutomationAsync(connection, automationId1, cancellationToken);
            await InsertAutomationAsync(connection, automationId2, cancellationToken);

            // Act
            var executionId1 = await _repository.GetOrCreateAsync(automationId1, eventId, cancellationToken);
            var executionId2 = await _repository.GetOrCreateAsync(automationId2, eventId, cancellationToken);

            // Assert
            Assert.NotEqual(executionId1, executionId2);
        }

        [Fact]
        public async Task GetOrCreateAsync_WhenTwoCallsRunConcurrently_ShouldCreateOnlyOneExecution()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;

            var automationId = Guid.NewGuid();
            var eventId = Guid.NewGuid();

            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await InsertAutomationAsync(connection, automationId, cancellationToken);

            // Act
            var firstAttempt = _repository.GetOrCreateAsync(automationId, eventId, cancellationToken);
            var secondAttempt = _repository.GetOrCreateAsync(automationId, eventId, cancellationToken);

            var results = await Task.WhenAll(firstAttempt, secondAttempt);

            // Assert
            Assert.Equal(results[0], results[1]);

            var row = await GetAutomationExecutionRowAsync(connection, automationId, eventId, cancellationToken);
            Assert.Equal(results[0], row.Id);
        }

        private sealed record ExecutionRow(
            Guid Id,
            Guid AutomationId,
            Guid? AutomationExecutorId,
            Guid EventId,
            DateTime StartedAt);

        private static async Task<ExecutionRow> GetAutomationExecutionRowAsync(
            DbConnection connection,
            Guid automationId,
            Guid eventId,
            CancellationToken cancellationToken)
        {
            return await connection.QuerySingleAsync<ExecutionRow>(
                new CommandDefinition(
                    """
                    SELECT Id, AutomationId, AutomationExecutorId, EventId, StartedAt
                    FROM AutomationExecutions
                    WHERE AutomationId = @AutomationId
                      AND EventId = @EventId
                    """,
                    new
                    {
                        AutomationId = automationId,
                        EventId = eventId
                    },
                    cancellationToken: cancellationToken));
        }

        private static async Task<DateTime> TimestampAsync(
            DbConnection connection,
            CancellationToken cancellationToken)
        {
            return await connection.QuerySingleAsync<DateTime>(
                new CommandDefinition(
                    "SELECT SYSUTCDATETIME();",
                    cancellationToken: cancellationToken));
        }

        private static async Task InsertAutomationAsync(
            DbConnection connection,
            Guid automationId,
            CancellationToken cancellationToken)
        {
            await connection.ExecuteAsync(
                new CommandDefinition(
                    """
                    INSERT INTO Automations
                        (Id, Name, Enabled, AutomationTriggerable)
                    VALUES
                        (@AutomationId, 'Test Automation', 1, 1);
                    """,
                    new { AutomationId = automationId },
                    cancellationToken: cancellationToken));
        }
    }
}
