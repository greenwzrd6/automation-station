using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Contracts;
using AutomationStation.Application.Models;
using AutomationStation.Core.Automations.Systems;
using AutomationStation.Core.Automations.Thens;
using AutomationStation.Infrastructure.Actions;

namespace AutomationStation.Unit.Tests.Infrastructure.Actions;

public sealed class ActionDispatcherTests
{
    [Fact]
    public async Task ExecuteAsync_CreatePlacementToTojSystem_IsBlocked()
    {
        // Arrange
        var handler = new FakeActionHandler(
            actionType: "CreatePlacement",
            targetSystem: TargetSystem.Kanban);

        var rateLimiter = new FakeActionRateLimiter();

        var automationExecutorRepository = new FakeAutomationExecutorRepository();

        var executor = new ActionDispatcher(
            [handler],
            rateLimiter,
            new ActionCatalog(),
            automationExecutorRepository);

        var then = new Then(
            Type: "CreatePlacement",
            TargetSystem: TargetSystem.TojSystem,
            ExecutorId: Guid.NewGuid(),
            Parameters: new Dictionary<string, string>());

        var context = CreateContext();

        // Act
        var exception = await Assert.ThrowsAsync<NotSupportedException>(
            () => executor.ExecuteAsync(
                then,
                context,
                CancellationToken.None));

        // Assert
        Assert.Contains(
            "not allowed",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);

        Assert.Equal(0, handler.ExecutionCount);
        Assert.Equal(0, rateLimiter.CallCount);
    }

    [Fact]
    public async Task ExecuteAsync_CreatePlacementToKanban_IsExecuted()
    {
        // Arrange
        var handler = new FakeActionHandler(
            actionType: "CreatePlacement",
            targetSystem: TargetSystem.Kanban);

        var rateLimiter = new FakeActionRateLimiter();

        var automationExecutorRepository = new FakeAutomationExecutorRepository();

        var executor = new ActionDispatcher(
            [handler],
            rateLimiter,
            new ActionCatalog(),
            automationExecutorRepository);

        var then = new Then(
            Type: "CreatePlacement",
            TargetSystem: TargetSystem.Kanban,
            ExecutorId: Guid.NewGuid(),
            Parameters: new Dictionary<string, string>());

        var context = CreateContext(companyId: 42);

        // Act
        await executor.ExecuteAsync(
            then,
            context,
            CancellationToken.None);

        // Assert
        Assert.Equal(1, handler.ExecutionCount);
        Assert.Equal(1, rateLimiter.CallCount);

        Assert.NotNull(rateLimiter.LastKey);
        Assert.Equal(42, rateLimiter.LastKey.CompanyId);
        Assert.Equal(
            "CreatePlacement",
            rateLimiter.LastKey.ActionType);
        Assert.Equal(
            TargetSystem.Kanban,
            rateLimiter.LastKey.TargetSystem);
    }

    [Fact]
    public async Task ExecuteAsync_CreatePlacementToKanban_ExceedsRateLimit_IsBlocked()
    {
        // Arrange
        var handler = new FakeActionHandler(
            actionType: "CreatePlacement",
            targetSystem: TargetSystem.Kanban);

        var rateLimiter = new FakeActionRateLimiter();

        var automationExecutorRepository = new FakeAutomationExecutorRepository();

        var executor = new ActionDispatcher(
            [handler],
            rateLimiter,
            new ActionCatalog(),
            automationExecutorRepository);

        var then = new Then(
            Type: "CreatePlacement",
            TargetSystem: TargetSystem.Kanban,
            ExecutorId: Guid.NewGuid(),
            Parameters: new Dictionary<string, string>());

        var context = CreateContext();

        // Act
        for (var i = 0; i < 100; i++)
        {
            await executor.ExecuteAsync(
                then,
                context,
                CancellationToken.None);
        }
        // The 101st execution should exceed the rate limit.
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => executor.ExecuteAsync(
                then,
                context,
                CancellationToken.None));

        // Assert
        Assert.Contains(
            "rate limit exceeded",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
        Assert.Equal(100, handler.ExecutionCount);
        Assert.Equal(101, rateLimiter.CallCount);
    }

    [Fact]
    public async Task ExecuteAsync_CreateTaskToToj_NoExecutor_IsBlocked()
    {
        // Arrange
        var handler = new FakeActionHandler(
            actionType: "CreateTask",
            targetSystem: TargetSystem.TojSystem);

        var rateLimiter = new FakeActionRateLimiter();

        var automationExecutorRepository = new FakeAutomationExecutorRepository();

        var executor = new ActionDispatcher(
            [handler],
            rateLimiter,
            new ActionCatalog(),
            automationExecutorRepository);

        var then = new Then(
            Type: "CreateTask",
            TargetSystem: TargetSystem.TojSystem,
            ExecutorId: null,
            Parameters: new Dictionary<string, string>());

        var context = CreateContext();

        // Act
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => executor.ExecuteAsync(
                    then,
                    context,
                    CancellationToken.None));

        // Assert
        Assert.Contains(
            "executor",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);

        Assert.Equal(0, handler.ExecutionCount);
        Assert.Equal(0, rateLimiter.CallCount);

    }

    [Fact]
    public async Task ExecuteAsync_CreateTaskToToj_ExecutorIdEmptyGuid_IsBlocked()
    {
        // Arrange
        var handler = new FakeActionHandler(
            actionType: "CreateTask",
            targetSystem: TargetSystem.TojSystem);

        var rateLimiter = new FakeActionRateLimiter();

        var automationExecutorRepository = new FakeAutomationExecutorRepository();

        var executor = new ActionDispatcher(
            [handler],
            rateLimiter,
            new ActionCatalog(),
            automationExecutorRepository);

        var then = new Then(
            Type: "CreateTask",
            TargetSystem: TargetSystem.TojSystem,
            ExecutorId: Guid.Empty, // Invalid GUID
            Parameters: new Dictionary<string, string>());

        var context = CreateContext();

        // Act
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => executor.ExecuteAsync(
                    then,
                    context,
                    CancellationToken.None));
        // Assert
        Assert.Contains(
            "executor",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);

        Assert.Equal(0, handler.ExecutionCount);
        Assert.Equal(0, rateLimiter.CallCount);
    }

    [Fact]
    public async Task ExecuteAsync_CreateTaskToToj_InvalidCompanyId_IsBlocked()
    {
        // Arrange
        var handler = new FakeActionHandler(
            actionType: "CreateTask",
            targetSystem: TargetSystem.TojSystem);

        var rateLimiter = new FakeActionRateLimiter();

        var automationExecutorRepository = 
            new FakeAutomationExecutorRepository
            {
                CompanyIdResult = 0
            };

        var executor = new ActionDispatcher(
            [handler],
            rateLimiter,
            new ActionCatalog(),
            automationExecutorRepository);

        var then = new Then(
            Type: "CreateTask",
            TargetSystem: TargetSystem.TojSystem,
            ExecutorId: Guid.NewGuid(),
            Parameters: new Dictionary<string, string>());

        var context = CreateContext();

        // Act
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => executor.ExecuteAsync(
                    then,
                    context,
                    CancellationToken.None));
        // Assert
        Assert.Contains(
            "company",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);

        Assert.Equal(0, handler.ExecutionCount);
        Assert.Equal(0, rateLimiter.CallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WithDisabledExecutor_IsBlocked()
    {
        // Arrange
        var handler = new FakeActionHandler(
            actionType: "CreateTask",
            targetSystem: TargetSystem.TojSystem);

        var rateLimiter = new FakeActionRateLimiter();

        var automationExecutorRepository = new FakeAutomationExecutorRepository
        {
            IsEnabledResult = false // Simulate disabled executor
        };

        var executor = new ActionDispatcher(
            [handler],
            rateLimiter,
            new ActionCatalog(),
            automationExecutorRepository);

        var then = new Then(
            Type: "CreateTask",
            TargetSystem: TargetSystem.TojSystem,
            ExecutorId: Guid.NewGuid(),
            Parameters: new Dictionary<string, string>());

        var context = CreateContext();

        // Act
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => executor.ExecuteAsync(
                    then,
                    context,
                    CancellationToken.None));
        // Assert
        Assert.Contains(
            "disabled",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);

        Assert.Equal(0, handler.ExecutionCount);
        Assert.Equal(0, rateLimiter.CallCount);
    }

    [Fact]
    public async Task ExecuteAsync_CreatePlacementToKanban_WithoutExecutor_IsExecuted()
    {
        // Arrange
        var handler = new FakeActionHandler(
            actionType: "CreatePlacement",
            targetSystem: TargetSystem.Kanban);

        var rateLimiter = new FakeActionRateLimiter();

        var automationExecutorRepository = new FakeAutomationExecutorRepository();

        var executor = new ActionDispatcher(
            [handler],
            rateLimiter,
            new ActionCatalog(),
            automationExecutorRepository);

        var then = new Then(
            Type: "CreatePlacement",
            TargetSystem: TargetSystem.Kanban,
            ExecutorId: null, // No executor specified
            Parameters: new Dictionary<string, string>());

        var context = CreateContext();

        // Act
        await executor.ExecuteAsync(
            then,
            context,
            CancellationToken.None);

        // Assert
        Assert.Equal(1, handler.ExecutionCount);
        Assert.Equal(1, rateLimiter.CallCount);
        Assert.NotNull(rateLimiter.LastKey);
        Assert.Equal(1, rateLimiter.LastKey.CompanyId);
        Assert.Equal(
            "CreatePlacement",
            rateLimiter.LastKey.ActionType);
        Assert.Equal(
            TargetSystem.Kanban,
            rateLimiter.LastKey.TargetSystem);
    }

    // Helper method to create a test ActionContext with a sample IntegrationEvent.
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

    // Fake implementations for testing.
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
                throw new InvalidOperationException("Rate limit exceeded.");
            }

            return Task.CompletedTask;
        }
    }

    private sealed class FakeAutomationExecutorRepository
            : IAutomationExecutorRepository
    {
        public bool IsEnabledResult { get; set; } = true;

        public int CallCount { get; private set; }

        public Guid? LastExecutorId { get; private set; }

        public int CompanyIdResult { get; set; } = 1;

        public Task<AutomationExecutor?> GetByIdAsync(
            Guid executorId,
            CancellationToken cancellationToken)
        {
            CallCount++;
            LastExecutorId = executorId;

            return Task.FromResult<AutomationExecutor?>(
                new AutomationExecutor(
                    Id: executorId,
                    CompanyId: CompanyIdResult,
                    Enabled: IsEnabledResult));
        }
    }
}