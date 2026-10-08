using AutomationStation.Application.Contracts;
using AutomationStation.Application.Models;
using AutomationStation.Application.Policies;
using AutomationStation.Core.Automations;
using AutomationStation.Core.Automations.Systems;

namespace AutomationStation.Unit.Tests.Application;

public class AutomationTriggerablePolicyTests
{
    [Fact]
    public async Task Evaluate_AutomationTriggerableTrue_IsAllowed()
    {
        // Arrange
        var policy = new AutomationTriggerablePolicy();

        var when = new When(
            "TestEvent",
            SourceSystem.Kanban,
            []);

        var thens = new[]
        {
            new Then(
                "TestAction",
                TargetSystem.Kanban,
                null,
                new Dictionary<string, string>())
        };

        var automation = new Automation(
            Guid.Empty,
            "TestAutomation",
            true,
            true,
            when,
            thens);

        var actor = new Actor(
            "",
            "Automation");

        var integrationEvent = new IntegrationEvent(
            Guid.Empty,
            "TestEvent",
            SourceSystem.Kanban,
            0,
            Guid.Empty,
            null,
            actor,
            new System.Text.Json.JsonElement());

        var context = new EvaluationContext(
            automation,
            integrationEvent);

        // Act
        var result = await policy.EvaluateAsync(
            context,
            CancellationToken.None);

        // Assert
        Assert.True(result.Allowed);
    }

    [Fact]
    public async Task Evaluate_AutomationTriggerableFalseWithUserAsActor_IsAllowed()
    {
        // Arrange
        var policy = new AutomationTriggerablePolicy();

        var when = new When(
            "TestEvent",
            SourceSystem.Kanban,
            []);

        var thens = new[]
        {
            new Then(
                "TestAction",
                TargetSystem.Kanban,
                null,
                new Dictionary<string, string>())
        };

        var automation = new Automation(
            Guid.Empty,
            "TestAutomation",
            true,
            false,
            when,
            thens);

        var actor = new Actor(
            "", 
            "User");

        var integrationEvent = new IntegrationEvent(
            Guid.Empty, 
            "TestEvent", 
            SourceSystem.Kanban, 
            0, 
            Guid.Empty, 
            null, 
            actor, 
            new System.Text.Json.JsonElement());

        var context = new EvaluationContext(
            automation,
            integrationEvent);

        // Act
        var result = await policy.EvaluateAsync(
            context,
            CancellationToken.None);

        // Assert
        Assert.True(result.Allowed);
    }

    [Fact]
    public async Task Evaluate_AutomationTriggerableFalseWithAutomationAsActor_IsBlocked()
    {
        // Arrange
        var policy = new AutomationTriggerablePolicy();

        var when = new When(
            "TestEvent",
            SourceSystem.Kanban,
            []);

        var thens = new[]
        {
            new Then(
                "TestAction",
                TargetSystem.Kanban,
                null,
                new Dictionary<string, string>())
        };

        var automation = new Automation(
            Guid.Empty,
            "TestAutomation",
            true,
            false,
            when,
            thens);

        var actor = new Actor(
            "",
            "Automation");

        var integrationEvent = new IntegrationEvent(
            Guid.Empty,
            "TestEvent",
            SourceSystem.Kanban,
            0,
            Guid.Empty,
            null,
            actor,
            new System.Text.Json.JsonElement());

        var context = new EvaluationContext(
            automation,
            integrationEvent);

        // Act
        var result = await policy.EvaluateAsync(
            context,
            CancellationToken.None);

        // Assert
        Assert.False(result.Allowed);
    }
}