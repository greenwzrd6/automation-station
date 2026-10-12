using AutomationStation.Application.Contracts;
using AutomationStation.Application.Models;
using AutomationStation.Application.Policies;
using AutomationStation.Core.Automations;
using AutomationStation.Core.Automations.Systems;

namespace AutomationStation.Unit.Tests.Application.Policies;

public class AutomationTriggerablePolicyTests
{
    [Fact]
    public void Evaluate_AutomationTriggerableTrue_IsAllowed()
    {
        // Arrange
        var policy = new AutomationTriggerablePolicy();

        var context = CreateContext(
            automationTriggerable: true,
            actorType: "Automation");

        // Act
        var result = policy.Evaluate(context);

        // Assert
        Assert.True(result.Allowed);
    }

    [Fact]
    public void Evaluate_AutomationTriggerableFalseWithUserAsActor_IsAllowed()
    {
        // Arrange
        var policy = new AutomationTriggerablePolicy();

        var context = CreateContext(
            automationTriggerable: false,
            actorType: "User");

        // Act
        var result = policy.Evaluate(context);

        // Assert
        Assert.True(result.Allowed);
    }

    [Fact]
    public void Evaluate_AutomationTriggerableFalseWithAutomationAsActor_IsBlocked()
    {
        // Arrange
        var policy = new AutomationTriggerablePolicy();

        var context = CreateContext(
            automationTriggerable: false,
            actorType: "Automation");

        // Act
        var result = policy.Evaluate(context);

        // Assert
        Assert.False(result.Allowed);
    }

    [Theory]
    [InlineData("Automation")]
    [InlineData("automation")]
    [InlineData("AUTOMATION")]
    [InlineData("aUtOmAtIoN")]
    public void Evaluate_AutomationActorTypeIsCaseInsensitive_IsAllowed(
        string actorType)
    {
        // Arrange
        var policy = new AutomationTriggerablePolicy();

        var context = CreateContext(
            automationTriggerable: true,
            actorType: actorType);

        // Act
        var result = policy.Evaluate(context);

        // Assert
        Assert.True(result.Allowed);
    }

    private static EvaluationContext CreateContext(
            bool automationTriggerable,
            string actorType)
    {
        var when = new When(
            "",
            SourceSystem.Kanban,
            []);

        var thens = new[]
        {
            new Then(
                "",
                TargetSystem.Kanban,
                null,
                new Dictionary<string, string>())
            };

        var automation = new Automation(
            Guid.Empty,
            "",
            true,
            automationTriggerable,
            when,
            thens);

        var actor = new Actor(
            "",
            actorType);

        var payload = System.Text.Json.JsonDocument
            .Parse("{}")
            .RootElement
            .Clone();

        var integrationEvent = new IntegrationEvent(
            Guid.Empty,
            "",
            SourceSystem.Kanban,
            0,
            Guid.Empty,
            null,
            actor,
            payload);

        return new EvaluationContext(automation, integrationEvent);
    }
}