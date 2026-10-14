using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Contracts;
using AutomationStation.Application.Models;
using AutomationStation.Core.Automations;
using AutomationStation.Core.Automations.Systems;
using AutomationStation.Infrastructure.Actions;
using AutomationStation.Infrastructure.Persistence;
using AutomationStation.Integration.Tests.Fixtures;
using Dapper;

namespace AutomationStation.Integration.Tests.Actions
{
    [Collection(DatabaseCollection.Name)]
    public class ActionExecutorTests(DatabaseFixture database)
    {
        private readonly DatabaseFixture _database = database;

        [Fact]
        public async Task ExecuteAsync_WithDisabledTojExecutor_ThrowsInvalidOperationException()
        {
            var cancellationToken = TestContext.Current.CancellationToken;
            var executorId = Guid.NewGuid();
            var handler = new FakeActionHandler(
                actionType: "CreateTask",
                targetSystem: TargetSystem.TojSystem);
            var executor = new ActionExecutor(
                handlers: [handler],
                rateLimiter: new FakeActionRateLimiter(),
                actionCatalog: new ActionCatalog(),
                executorRepository: new AutomationExecutorRepository(
                    _database.ConnectionFactory));

            // Arrange: spara en avstängd executor.
            using (var connection = _database.ConnectionFactory.CreateConnection())
            {
                await connection.OpenAsync(cancellationToken);

                await connection.ExecuteAsync(
                    new CommandDefinition(
                        """
                        INSERT INTO AutomationExecutors
                            (Id, CompanyId, TargetSystem, Name, Enabled)
                        VALUES
                            (@Id, 1, @TargetSystem, @Name, 0);
                        """,
                        new
                        {
                            Id = executorId,
                            TargetSystem = TargetSystem.TojSystem,
                            Name = "Disabled test executor"
                        },
                        cancellationToken: cancellationToken));
            }

            var then = new Then(
                Type: "CreateTask",
                TargetSystem: TargetSystem.TojSystem,
                ExecutorId: executorId,
                Parameters: new Dictionary<string, string>());

            var context = CreateContext();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => executor.ExecuteAsync(then, context, cancellationToken));

            Assert.Contains("disabled", exception.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(0, handler.ExecutionCount);

        }

        [Fact]
        public async Task ExecuteAsync_WithEnabledTojExecutor_ExecutesSuccessfully()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;
            var executorId = Guid.NewGuid();
            var handler = new FakeActionHandler(
                actionType: "CreateTask",
                targetSystem: TargetSystem.TojSystem);
            var executor = new ActionExecutor(
                handlers: [handler],
                rateLimiter: new FakeActionRateLimiter(),
                actionCatalog: new ActionCatalog(),
                executorRepository: new AutomationExecutorRepository(
                    _database.ConnectionFactory));
            // Arrange: spara en aktiverad executor.
            using (var connection = _database.ConnectionFactory.CreateConnection())
            {
                await connection.OpenAsync(cancellationToken);
                await connection.ExecuteAsync(
                                new CommandDefinition(
                                    """
                        INSERT INTO AutomationExecutors
                            (Id, CompanyId, TargetSystem, Name, Enabled)
                        VALUES
                            (@Id, 1, @TargetSystem, @Name, 1);
                        """,
                                    new
                                    {
                                        Id = executorId,
                                        TargetSystem = TargetSystem.TojSystem,
                                        Name = "Enabled test executor"
                                    },
                                    cancellationToken: cancellationToken));
            }
            var then = new Then(
                Type: "CreateTask",
                TargetSystem: TargetSystem.TojSystem,
                ExecutorId: executorId,
                Parameters: new Dictionary<string, string>());


            var context = CreateContext();

            // Act
            await executor.ExecuteAsync(then, context, cancellationToken);

            // Assert
            Assert.Equal(1, handler.ExecutionCount);
        }

        [Fact]
        public async Task ExecuteAsync_WithNonExistingExecutor_ThrowsInvalidOperationException()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;
            var nonExistingExecutorId = Guid.NewGuid();
            var handler = new FakeActionHandler(
                actionType: "CreateTask",
                targetSystem: TargetSystem.TojSystem);
            var executor = new ActionExecutor(
                handlers: [handler],
                rateLimiter: new FakeActionRateLimiter(),
                actionCatalog: new ActionCatalog(),
                executorRepository: new AutomationExecutorRepository(
                    _database.ConnectionFactory));
            var then = new Then(
                Type: "CreateTask",
                TargetSystem: TargetSystem.TojSystem,
                ExecutorId: nonExistingExecutorId,
                Parameters: new Dictionary<string, string>());
            var context = CreateContext();

            // Act
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => executor.ExecuteAsync(then, context, cancellationToken));

            // Assert
            Assert.Contains("does not exist", exception.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(0, handler.ExecutionCount);
        }

        // Helper method to create a sample ActionContext for testing
        private static ActionContext CreateContext(int companyId = 1)
        {
            var integrationEvent = new IntegrationEvent(
                EventId: Guid.NewGuid(),
                EventType: "PlacementCreated",
                Source: default!,
                CompanyId: companyId,
                CorrelationId: Guid.NewGuid(),
                CausationEventId: null,
                Actor: default!,
                Payload: default);

            return new ActionContext(
                Event: integrationEvent,
                ExecutionId: Guid.NewGuid());
        }

        // Fake implementations for testing
        private sealed class FakeActionHandler(
            string actionType,
            TargetSystem targetSystem)
            : IActionHandler
        {
            public string ActionType { get; } = actionType;

            public TargetSystem TargetSystem { get; } = targetSystem;

            public int ExecutionCount { get; private set; }

            public Task ExecuteAsync(
                Then then,
                ActionContext context,
                CancellationToken cancellationToken)
            {
                ExecutionCount++;

                return Task.CompletedTask;
            }
        }

        private sealed class FakeActionRateLimiter
            : IActionRateLimiter
        {
            public int CallCount { get; private set; }
            public ActionRateLimitKey? LastKey { get; private set; }

            public Task WaitAsync(
                ActionRateLimitKey key,
                int permittedActions,
                TimeSpan window,
                CancellationToken cancellationToken)
            {
                CallCount++;
                LastKey = key;

                if (CallCount > permittedActions)
                {
                    throw new InvalidOperationException(
                        "Rate limit exceeded.");
                }

                return Task.CompletedTask;
            }
        }
    }
}
