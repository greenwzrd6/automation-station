using AutomationStation.Core.Automations.Systems;

namespace AutomationStation.Core.Automations
{
    public sealed record When(
        string EventType,
        SourceSystem EventSource,
        IReadOnlyCollection<Condition> Conditions);
}
