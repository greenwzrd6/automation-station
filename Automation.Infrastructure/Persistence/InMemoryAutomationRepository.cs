using Automation.Application.Abstractions;
using Automation.Core.Automations;

namespace Automation.Infrastructure.Persistence;

public sealed class InMemoryAutomationRepository : IAutomationRepository
{
    public Task<IReadOnlyCollection<AutomationRule>>
        GetEnabledByEventTypeAsync(
            string eventType,
            CancellationToken cancellationToken)
    {
        var automation = new AutomationRule(
            id: Guid.NewGuid(),
            name: "Move to testing",
            isEnabled: true,
            when: new When(
                EventType: "PlacementCreated",
                EventSource: "Planning",
                Conditions:
                [
                    new Condition(
                        Field: "columnId",
                        Operator: ConditionOperator.Equals,
                        Value: "22222222-2222-2222-2222-222222222224")
                ]),
            thens:
            [
                new Then(
                    Type: "CreatePlacement",
                    Parameters: new Dictionary<string, string>
                    {
                        ["boardId"] =
                            "11111111-1111-1111-1111-111111111112",

                        ["columnId"] =
                            "22222222-2222-2222-2222-222222222226"
                    })
            ]);

        IReadOnlyCollection<AutomationRule> automations =
            eventType == "PlacementCreated"
                ? [automation]
                : [];

        return Task.FromResult(automations);
    }
}