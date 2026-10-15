using AutomationStation.Application.Contracts;
using AutomationStation.Application.Models;
using AutomationStation.Core.Automations;
using AutomationStation.Core.Automations.Systems;
using AutomationStation.Core.Automations.Whens;
using AutomationStation.Infrastructure.Persistence;
using AutomationStation.Integration.Tests.Fixtures;
using Dapper;
using System.Data.Common;
using System.Text.Json;

namespace AutomationStation.Integration.Tests.Messaging
{
    /// <summary>
    /// Class <c>HistoryRepositoryTests</c> is the test class for <c>HistoryRepository</c>
    /// </summary>
    /// <param name="database">Primary constructor for <c>DatabaseFixture</c></param>
    [Collection(DatabaseCollection.Name)]
    public sealed class HistoryRepositoryTests(DatabaseFixture database) : IAsyncLifetime
    {
        private readonly DatabaseFixture _database = database;
        private readonly HistoryRepository _repository = new(
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
        public async Task CreateAutomationTimestampAsync_WhenUsed_CreatesCorrectAutomationHistory()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;

            var automationId = Guid.NewGuid();
            var correlationId = Guid.NewGuid();

            var context = CreateContext(automationId, correlationId);

            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            // Fake automation
            await InsertAutomationAsync(connection, automationId, cancellationToken);

            var beforeInsert = await TimestampAsync(connection, cancellationToken);

            // Act
            // Insert into history
            await _repository.CreateAutomationTimestampAsync(
                context,
                cancellationToken);

            // Assert
            var afterInsert = await TimestampAsync(connection, cancellationToken);

            var automationRow = await GetAutomationHistoryWithAutomationIdAsync(connection, automationId, cancellationToken);

            Assert.Equal(automationId, automationRow.AutomationId);

            Assert.Equal(correlationId, automationRow.CorrelationId);

            Assert.Equal("-1", automationRow.ActorId);

            Assert.NotEqual(Guid.Empty, automationRow.Id);

            Assert.Null(automationRow.CausationEventId);

            Assert.InRange(automationRow.TriggeredAt, beforeInsert.AddSeconds(-1), afterInsert.AddSeconds(1));
        }

        [Fact]
        public async Task CreateAutomationTimestampAsync_WhenProvidingCausationEventId_SavesCorrectCausationEventId()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;

            var automationId = Guid.NewGuid();
            var correlationId = Guid.NewGuid();
            var causationEventId = Guid.NewGuid();

            var context = CreateContext(automationId, correlationId, causationEventId);

            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            // Fake automation
            await InsertAutomationAsync(connection, automationId, cancellationToken);

            // Act
            // Insert into history
            await _repository.CreateAutomationTimestampAsync(
                context,
                cancellationToken);

            // Assert
            var automationRow = await GetAutomationHistoryWithAutomationIdAsync(connection, automationId, cancellationToken);

            Assert.Equal(causationEventId, automationRow.CausationEventId);
        }

        [Fact]
        public async Task EventAlreadyProcessedAsync_WhenCorrelationIdDiffers_ShouldReturnFalse()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;

            var automationId = Guid.NewGuid();
            var correlationId = Guid.NewGuid();

            var context = CreateContext(automationId, correlationId);

            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            // Fake automation
            await InsertAutomationAsync(connection, automationId, cancellationToken);

            // Insert into history
            await _repository.CreateAutomationTimestampAsync(
                context,
                cancellationToken);

            // Act
            var resultAfterInsert = await _repository.EventAlreadyProcessedAsync(
                automationId,
                CorrelationId: Guid.NewGuid(),
                cancellationToken);

            // Assert
            Assert.False(resultAfterInsert);
        }

        [Fact]
        public async Task EventAlreadyProcessedAsync_WhenAutomationIdDiffers_ShouldReturnFalse()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;

            var automationId = Guid.NewGuid();
            var correlationId = Guid.NewGuid();

            var context = CreateContext(automationId, correlationId);

            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            // Fake automation
            await InsertAutomationAsync(connection, automationId, cancellationToken);

            // Insert into history
            await _repository.CreateAutomationTimestampAsync(
                context,
                cancellationToken);

            // Act
            var resultAfterInsert = await _repository.EventAlreadyProcessedAsync(
                AutomationId: Guid.NewGuid(),
                correlationId,
                cancellationToken);

            //Assert
            Assert.False(resultAfterInsert);
        }

        [Fact]
        public async Task EventAlreadyProcessedAsync_WhenUsed_ReturnsCorrectValue()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;

            var automationId = Guid.NewGuid();
            var correlationId = Guid.NewGuid();

            var context = CreateContext(automationId, correlationId);

            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            // Fake automation
            await InsertAutomationAsync(connection, automationId, cancellationToken);

            // Act
            var resultBeforeInsert = await _repository.EventAlreadyProcessedAsync(
                automationId,
                correlationId,
                cancellationToken);

            //Assert
            Assert.False(resultBeforeInsert);

            // Arrange
            // Insert into history
            await _repository.CreateAutomationTimestampAsync(
                context,
                cancellationToken);

            // Act
            var resultAfterInsert = await _repository.EventAlreadyProcessedAsync(
                automationId,
                correlationId,
                cancellationToken);

            //Assert
            Assert.True(resultAfterInsert);
        }

        private sealed record HistoryRow(
            Guid Id,
            Guid AutomationId,
            string ActorId,
            Guid CorrelationId,
            Guid? CausationEventId,
            DateTime TriggeredAt);

        private static async Task<HistoryRow> GetAutomationHistoryWithAutomationIdAsync(
            DbConnection connection,
            Guid automationId,
            CancellationToken cancellationToken)
        {
            return await connection.QuerySingleAsync<HistoryRow>(
                    new CommandDefinition(
                        """
                        SELECT Id, AutomationId, ActorId, CorrelationId, CausationEventId, TriggeredAt
                        FROM AutomationHistory
                        WHERE AutomationId = @AutomationId;
                        """,
                        new { AutomationId = automationId },
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

        private static EvaluationContext CreateContext(
            Guid automationId,
            Guid correlationId,
            Guid? causationEventId = null)
        {
            var automation = new Automation(
                id: automationId,
                name: "Test Automation",
                isEnabled: true,
                automationTriggerable: true,
                when: new When(new Trigger(EventType: "PlacementCreated",
                    SourceSystem: SourceSystem.Kanban),
                    Conditions: []),
                thens: []);

            var integrationEvent = new IntegrationEvent(
                EventId: Guid.NewGuid(),
                EventType: "PlacementCreated",
                Source: SourceSystem.Kanban,
                CompanyId: 1,
                CorrelationId: correlationId,
                CausationEventId: causationEventId,
                Actor: new Actor("-1", "User"),
                Payload: JsonSerializer.SerializeToElement(new { }));

            return new EvaluationContext(
                automation,
                integrationEvent);
        }
    }
}
