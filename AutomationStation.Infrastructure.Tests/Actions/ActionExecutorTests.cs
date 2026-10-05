using AutomationStation.Application.Abstractions;
using AutomationStation.Application.Contracts;
using AutomationStation.Application.Models;
using AutomationStation.Core.Automations;
using AutomationStation.Core.Automations.Systems;
using AutomationStation.Infrastructure.Actions;
using System.Text.Json;

namespace AutomationStation.Tests.Actions;

public sealed class ActionExecutorTests
{
    [Fact]
    public async Task ExecuteAsync_CreatePlacementToTojSystem_IsBlocked()
    {
        // Arrange
        var handler = new FakeActionHandler(
            actionType: "CreatePlacement",
            targetSystem: TargetSystem.Kanban);

        var rateLimiter = new FakeActionRateLimiter();

        var executor = new ActionExecutor(
            [handler],
            rateLimiter,
            new ActionCatalog());

        var then = new Then(
            Type: "CreatePlacement",
            TargetSystem: TargetSystem.TojSystem,
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

        var executor = new ActionExecutor(
            [handler],
            rateLimiter,
            new ActionCatalog());

        var then = new Then(
            Type: "CreatePlacement",
            TargetSystem: TargetSystem.Kanban,
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

    private static AutomationActionContext CreateContext(
        int companyId = 1)
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

        return new AutomationActionContext(
            Event: integrationEvent,
            CorrelationId: Guid.NewGuid(),
            ExecutionId: Guid.NewGuid());
    }

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
            AutomationActionContext context,
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

            return Task.CompletedTask;
        }
    }
}