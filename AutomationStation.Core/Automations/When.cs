using AutomationStation.Core.Automations.Systems;

namespace AutomationStation.Core.Automations
{

    /// <summary>
    /// Record <c>When</c> represents when something should happen inside an automation.
    /// </summary>
    /// <param name="EventType">The event that triggers the <c>When</c>, e.g. "PlacementCreated".</param>
    /// <param name="EventSource">The system the event came from <see cref="SourceSystem"/>.</param>
    /// <param name="Conditions">The conditions that must be met for the automation to execute. See <see cref="Condition"/>.</param>
    public sealed record When(
        string EventType,
        SourceSystem EventSource,
        IReadOnlyCollection<Condition> Conditions);
}
