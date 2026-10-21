using Dapper;

using System.Data.Common;
using System.Text.Json;

using AutomationStation.Application.Contracts;
using AutomationStation.Application.Models;
using Actor = AutomationStation.Application.Models.Actor;
using AutomationStation.Core.Automations;
using AutomationStation.Core.Automations.Systems;
using AutomationStation.Core.Automations.Whens;
using AutomationStation.Infrastructure.Limiting;
using AutomationStation.Integration.Tests.Fixtures;

namespace AutomationStation.Integration.Tests.Messaging
{
    /// <summary>
    /// Class <c>AutomationExecutionLimiterTests</c> is the test class for <c>AutomationExecutionLimiter</c>
    /// </summary>
    /// <param name="database">Primary constructor for <c>DatabaseFixture</c></param>
    [Collection(DatabaseCollection.Name)]
    public sealed class AutomationExecutionLimiterTests(DatabaseFixture database) : IAsyncLifetime
    {
        // Initializes the DatabaseFixture and repository
        private readonly DatabaseFixture _database = database;
        private readonly AutomationExecutionLimiter _repository = new(
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
        public async Task TryRecordExecutionAsync_WhenCorrelationReachesAutomationLimit_ShouldRejectNextExecution()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;

            // The limit currently used in correlationlooprepo
            const int limit = 10000;

            var automationId = Guid.NewGuid();
            var correlationId = Guid.NewGuid();

            // Creates a connection to the testdatabase
            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await InsertAutomationAsync(
                connection,
                automationId,
                cancellationToken);

            // Seed limit - 1 executions inside the time window.
            await InsertAutomationHistory(
                connection,
                automationId,
                correlationId,
                (limit - 1),
                cancellationToken);

            // Act
            // Check if the next execution is blocked (should not be blocked yet)
            var recordedAtLimit = await _repository.TryRecordExecutionAsync(
                CreateContext(automationId, correlationId),
                cancellationToken);

            // Assert
            Assert.True(recordedAtLimit);

            //Act
            // Check if the next execution is blocked (should be blocked now)
            var recordedOverLimit = await _repository.TryRecordExecutionAsync(
                CreateContext(automationId, correlationId),
                cancellationToken);

            // Assert
            Assert.False(recordedOverLimit);

            var historyCount = await CountHistoryRows(
                connection,
                automationId,
                correlationId,
                cancellationToken);

            Assert.Equal(limit, historyCount);
        }

        [Fact]
        public async Task TryRecordExecutionAsync_WhenNoHistory_ShouldAllowAndSaveFirstExecution()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;

            var automationId = Guid.NewGuid();
            var correlationId = Guid.NewGuid();

            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await InsertAutomationAsync(
                connection,
                automationId,
                cancellationToken);

            // Act
            var recorded = await _repository.TryRecordExecutionAsync(
                CreateContext(automationId, correlationId),
                cancellationToken);

            // Assert
            Assert.True(recorded);

            var historyCount = await CountHistoryRows(
                connection,
                automationId,
                correlationId,
                cancellationToken);

            Assert.Equal(1, historyCount);
        }

        [Fact]
        public async Task TryRecordExecutionAsync_WhenUsingDifferentCorrelationId_ShouldAllowNewExecution()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;

            // The limit currently used in correlationlooprepo
            const int limit = 10000;

            var automationId = Guid.NewGuid();
            var correlationId1 = Guid.NewGuid();
            var correlationId2 = Guid.NewGuid();

            // Creates a connection to the testdatabase
            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await InsertAutomationAsync(
                connection,
                automationId,
                cancellationToken);

            await InsertAutomationHistory(
                connection,
                automationId,
                correlationId1,
                limit,
                cancellationToken);

            // Act
            var recordedOverLimit = await _repository.TryRecordExecutionAsync(
                CreateContext(automationId, correlationId2),
                cancellationToken);

            // Assert: Should be allowed to insert with new correlationId
            Assert.True(recordedOverLimit);

            // Assert: Should be one history row in automationhistory with the new correlationid
            var historyCount = await CountHistoryRows(
                connection,
                automationId,
                correlationId2,
                cancellationToken);

            Assert.Equal(1, historyCount);
        }

        [Fact]
        public async Task TryRecordExecutionAsync_WhenUsingDifferentAutomation_ShouldAllowNewExecution()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;

            // The limit currently used in correlationlooprepo
            const int limit = 10000;

            var automationId1 = Guid.NewGuid();
            var automationId2 = Guid.NewGuid();
            var correlationId = Guid.NewGuid();

            // Creates a connection to the testdatabase
            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await InsertAutomationAsync(
                connection,
                automationId1,
                cancellationToken);

            await InsertAutomationAsync(
                connection,
                automationId2,
                cancellationToken);

            await InsertAutomationHistory(
                connection,
                automationId1,
                correlationId,
                limit,
                cancellationToken);

            // Act      
            var recordedOverLimit = await _repository.TryRecordExecutionAsync(
                CreateContext(automationId2, correlationId),
                cancellationToken);

            // Assert: Should be allowed to insert with new Automation
            Assert.True(recordedOverLimit);

            // Assert: Should be one history row in automationhistory with the new correlationid
            var historyCount = await CountHistoryRows(
                connection,
                automationId2,
                correlationId,
                cancellationToken);

            Assert.Equal(1, historyCount);
        }

        [Fact]
        public async Task TryRecordExecutionAsync_WhenHistoryIsOlderThanThreeMinutes_ShouldAllowExecution()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;

            // The limit currently used in correlationlooprepo
            const int limit = 10000;

            var automationId = Guid.NewGuid();
            var correlationId = Guid.NewGuid();

            // Creates a connection to the testdatabase
            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await InsertAutomationAsync(
                connection,
                automationId,
                cancellationToken);

            // Inserts four minutes old history
            await InsertAutomationHistory(
                connection,
                automationId,
                correlationId,
                limit,
                cancellationToken,
                minutesAgo: 4);

            // Act  
            var recordedOverLimit = await _repository.TryRecordExecutionAsync(
                CreateContext(automationId, correlationId),
                cancellationToken);

            // Assert
            Assert.True(recordedOverLimit);

            var historyCount = await CountHistoryRows(
                connection,
                automationId,
                correlationId,
                cancellationToken);

            Assert.Equal(limit + 1, historyCount);
        }

        [Fact]
        public async Task TryRecordExecutionAsync_WhenTwoRequestsCompeteForLastSlot_ShouldOnlyAllowOne()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;

            // The limit currently used in correlationlooprepo
            const int limit = 10000;

            var automationId = Guid.NewGuid();
            var correlationId = Guid.NewGuid();

            // Creates a connection to the testdatabase
            using var connection = _database.ConnectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await InsertAutomationAsync(
                connection,
                automationId,
                cancellationToken);

            // Seed limit - 1 executions inside the time window.
            await InsertAutomationHistory(
                connection,
                automationId,
                correlationId,
                (limit - 1),
                cancellationToken);

            // Act
            var firstAttempt = _repository.TryRecordExecutionAsync(
                CreateContext(automationId, correlationId),
                cancellationToken);

            var secondAttempt = _repository.TryRecordExecutionAsync(
                CreateContext(automationId, correlationId),
                cancellationToken);

            // run both at the same time
            var results = await Task.WhenAll(firstAttempt, secondAttempt);

            // Assert
            Assert.Single(results, recorded => recorded);
            Assert.Single(results, recorded => !recorded);

            var historyCount = await CountHistoryRows(
                connection,
                automationId,
                correlationId,
                cancellationToken);

            Assert.Equal(limit, historyCount);
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
                    new
                    {
                        AutomationId = automationId
                    },
                    cancellationToken: cancellationToken));
        }

        private static async Task InsertAutomationHistory(
            DbConnection connection,
            Guid automationId,
            Guid correlationId,
            int limit,
            CancellationToken cancellationToken,
            int minutesAgo = 0)
        {
            await connection.ExecuteAsync(
                    new CommandDefinition(
                    """
                    ;WITH Numbers AS (
                        SELECT 1 AS Number
                        UNION ALL
                        SELECT Number + 1
                        FROM Numbers
                        WHERE Number < @Count
                    )
                    INSERT INTO AutomationHistory
                        (Id, AutomationId, ActorId, CorrelationId, CausationEventId, TriggeredAt)
                    SELECT
                        NEWID(),
                        @AutomationId,
                        @ActorId,
                        @CorrelationId,
                        NULL,
                        DATEADD(MINUTE, -@MinutesAgo, SYSUTCDATETIME())
                    FROM Numbers
                    OPTION (MAXRECURSION 0);
                    """,
                    new
                    {
                        AutomationId = automationId,
                        ActorId = "-1",
                        CorrelationId = correlationId,
                        Count = limit,
                        MinutesAgo = minutesAgo
                    },
                    cancellationToken: cancellationToken));
        }

        private static async Task<int> CountHistoryRows(
            DbConnection connection,
            Guid automationId,
            Guid correlationId,
            CancellationToken cancellationToken)
        {
            return await connection.QuerySingleAsync<int>(
                    new CommandDefinition(
                    """
                    SELECT COUNT(*)
                    FROM AutomationHistory
                    WHERE AutomationId = @AutomationId
                      AND CorrelationId = @CorrelationId;
                    """,
                    new
                    {
                        AutomationId = automationId,
                        CorrelationId = correlationId
                    },
                    cancellationToken: cancellationToken));
        }

        private static EvaluationContext CreateContext(Guid automationId, Guid correlationId)
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
                CausationEventId: null,
                Actor: new Actor("-1", "User"),
                Payload: JsonSerializer.SerializeToElement(new { }));

            return new EvaluationContext(
                automation,
                integrationEvent);
        }
    }
}
