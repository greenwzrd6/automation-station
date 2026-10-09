
//using AutomationStation.Application.Abstractions;
//using AutomationStation.Application.Contracts;
//using AutomationStation.Application.Models;
//using AutomationStation.Core.Automations;
//using AutomationStation.Core.Automations.Systems;
//using AutomationStation.Infrastructure.Database;
//using AutomationStation.Infrastructure.Persistence;
//using Dapper;
//using Microsoft.Extensions.Configuration;
//using System.Text.Json;

//namespace AutomationStation.Infrastructure.Tests
//{
//    public sealed class CorrelationLoopTests
//    {
//        [Fact]
//        public async Task IsBlockedAsync_ShouldReturnTrue_WhenLoopDetected()
//        {
//            // Arrange
//            var configuration = new ConfigurationBuilder()
//                .SetBasePath(Directory.GetCurrentDirectory())
//                .AddJsonFile("appsettings.json")
//                .Build();

//            var connectionFactory = new DbConnectionFactory(configuration);
//            var repository = new CorrelationLoopRepository(connectionFactory);

//            // The limit currently used in correlationlooprepo
//            const int limit = 10000;

//            var automationId = Guid.NewGuid();
//            var actorId = $"test-{Guid.NewGuid()}";

//            using var connection = connectionFactory.CreateConnection();
//            await connection.OpenAsync();

//            // Act
//            try
//            {
//                await connection.ExecuteAsync(
//                    """
//                    INSERT INTO Automations
//                        (Id, Name, IsEnabled, AutomationTriggerable)
//                    VALUES
//                        (@Id, 'Test Automation', 1, 1)
//                    """,
//                    new { AutomationId = automationId });

//                // Seed limit - 1 executions inside the time window.
//                await connection.ExecuteAsync(
//                    """
//                    ;WITH Numbers AS (
//                        SELECT 1 AS Number
//                        UNION ALL
//                        SELECT Number + 1
//                        FROM Numbers
//                        WHERE Number < @Count
//                    )
//                    INSERT INTO AutomationHistory
//                        (Id, AutomationId, ActorId, CorrelationId,
//                         CausationEventId, TriggeredAt)
//                    SELECT
//                        NEWID(),
//                        @AutomationId,
//                        @ActorId,
//                        NEWID(),
//                        NULL,
//                        SYSUTCDATETIME()
//                    FROM Numbers
//                    OPTION (MAXRECURSION 0);
//                    """,
//                    new
//                    {
//                        AutomationId = automationId,
//                        ActorId = actorId,
//                        Count = limit - 1
//                    });
//            }
//            // Assert
//            Assert.False(blocked);
//        }

//        private static EvaluationContext CreateContext(Guid automationId, string actorId)
//        {
//            var automation = new Automation(
//                id: automationId,
//                name: "Test Automation",
//                isEnabled: true,
//                automationTriggerable: true,
//                when: new When(
//                    EventType: "PlacementCreated",
//                    EventSource: SourceSystem.Kanban,
//                    Conditions: []),
//                thens: []);

//            var integrationEvent = new IntegrationEvent(
//                EventId: Guid.NewGuid(),
//                EventType: "PlacementCreated",
//                Source: SourceSystem.Kanban,
//                CompanyId: 1,
//                CorrelationId: Guid.NewGuid(),
//                CausationEventId: null,
//                Actor: new Actor(actorId, "User"),
//                Payload: JsonSerializer.SerializeToElement(new { }));

//            return new EvaluationContext(
//                automation,
//                integrationEvent);
//        }
//    }
//}
